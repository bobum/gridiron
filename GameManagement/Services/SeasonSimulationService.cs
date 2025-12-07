using DataAccessLayer.Repositories;
using Microsoft.EntityFrameworkCore;
using DomainObjects;
using Microsoft.Extensions.Logging;

namespace GameManagement.Services;

public class SeasonSimulationService : ISeasonSimulationService
{
    private readonly ISeasonRepository _seasonRepository;
    private readonly IGameRepository _gameRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly IEngineSimulationService _engineSimulationService;
    private readonly ILogger<SeasonSimulationService> _logger;

    public SeasonSimulationService(
        ISeasonRepository seasonRepository,
        IGameRepository gameRepository,
        ITeamRepository teamRepository,
        IEngineSimulationService engineSimulationService,
        ILogger<SeasonSimulationService> logger)
    {
        _seasonRepository = seasonRepository;
        _gameRepository = gameRepository;
        _teamRepository = teamRepository;
        _engineSimulationService = engineSimulationService;
        _logger = logger;
    }

    public async Task<SeasonSimulationResult> SimulateCurrentWeekAsync(int seasonId)
    {
        try
        {
            var season = await _seasonRepository.GetByIdWithWeeksAndGamesAsync(seasonId);
            if (season == null)
            {
                return new SeasonSimulationResult { Error = $"Season {seasonId} not found" };
            }

            if (season.IsComplete)
            {
                return new SeasonSimulationResult { Error = "Season is already complete" };
            }

            var currentWeek = season.Weeks.FirstOrDefault(w => w.WeekNumber == season.CurrentWeek);
            if (currentWeek == null)
            {
                return new SeasonSimulationResult { Error = $"Week {season.CurrentWeek} not found in season {seasonId}" };
            }

            if (currentWeek.Status == WeekStatus.Completed)
            {
                // If current week is already marked complete, try to advance to next week
                // This handles cases where simulation might have been interrupted or manually advanced
                if (!AdvanceToNextWeek(season))
                {
                    return new SeasonSimulationResult 
                    { 
                        SeasonId = seasonId,
                        WeekNumber = season.CurrentWeek,
                        SeasonCompleted = true,
                        Error = "Season is complete (no more weeks)"
                    };
                }
                
                // Get the new current week
                currentWeek = season.Weeks.FirstOrDefault(w => w.WeekNumber == season.CurrentWeek);
                if (currentWeek == null)
                {
                    return new SeasonSimulationResult { Error = $"Next week {season.CurrentWeek} not found" };
                }
            }

            currentWeek.Status = WeekStatus.InProgress;
            await _seasonRepository.UpdateAsync(season); // Save status change

            var results = new List<GameSimulationResult>();
            var unplayedGames = currentWeek.Games.Where(g => !g.IsComplete).ToList();

            _logger.LogInformation("Simulating {Count} games for Season {SeasonId} Week {Week}", 
                unplayedGames.Count, seasonId, season.CurrentWeek);

            foreach (var game in unplayedGames)
            {
                // We need to load full team data for simulation
                var fullGame = await _gameRepository.GetByIdWithTeamsAndPlayersAsync(game.Id);
                if (fullGame == null || fullGame.HomeTeam == null || fullGame.AwayTeam == null)
                {
                    _logger.LogWarning("Skipping game {GameId}: Team data missing", game.Id);
                    continue;
                }

                // Run simulation
                var simResult = _engineSimulationService.SimulateGame(fullGame.HomeTeam, fullGame.AwayTeam);

                // Update game record
                fullGame.HomeScore = simResult.HomeScore;
                fullGame.AwayScore = simResult.AwayScore;
                fullGame.IsComplete = true;
                fullGame.PlayedAt = DateTime.UtcNow;
                fullGame.RandomSeed = simResult.RandomSeed;
                
                // Note: PlayByPlay is typically large, we might want to store it separately or compressed
                // For now, we'll assume the repository handles it or we map it if needed
                // fullGame.PlayByPlay = ... (Engine result needs to be mapped to Domain PlayByPlay if we want to save it)

                await _gameRepository.UpdateAsync(fullGame);

                results.Add(new GameSimulationResult
                {
                    GameId = fullGame.Id,
                    HomeTeam = fullGame.HomeTeam.Name,
                    AwayTeam = fullGame.AwayTeam.Name,
                    HomeScore = fullGame.HomeScore,
                    AwayScore = fullGame.AwayScore,
                    IsTie = simResult.IsTie
                });
            }

            // Mark week as complete
            currentWeek.Status = WeekStatus.Completed;
            currentWeek.CompletedDate = DateTime.UtcNow;

            // Advance season pointer
            bool seasonEnded = !AdvanceToNextWeek(season);
            
            await _seasonRepository.UpdateAsync(season);
            await _seasonRepository.SaveChangesAsync();

            return new SeasonSimulationResult
            {
                SeasonId = seasonId,
                WeekNumber = currentWeek.WeekNumber,
                GamesSimulated = results.Count,
                GameResults = results,
                SeasonCompleted = seasonEnded
            };
        }
        catch (DbUpdateConcurrencyException)
        {
            _logger.LogWarning("Concurrency conflict simulating week for season {SeasonId}", seasonId);
            return new SeasonSimulationResult 
            { 
                Error = "Simulation failed due to concurrent modification. Please try again.",
                IsConcurrencyError = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error simulating week for season {SeasonId}", seasonId);
            return new SeasonSimulationResult { Error = $"Simulation failed: {ex.Message}" };
        }
    }

    private bool AdvanceToNextWeek(Season season)
    {
        var nextWeekNumber = season.CurrentWeek + 1;
        var nextWeek = season.Weeks.FirstOrDefault(w => w.WeekNumber == nextWeekNumber);

        if (nextWeek != null)
        {
            season.CurrentWeek = nextWeekNumber;
            // Update phase if next week belongs to a different phase
            if (nextWeek.Phase != season.Phase)
            {
                season.Phase = nextWeek.Phase;
            }
            return true;
        }
        else
        {
            // No more weeks, season is complete
            season.IsComplete = true;
            return false;
        }
    }

    public async Task<SeasonSimulationResult> RevertLastWeekAsync(int seasonId)
    {
        try
        {
            var season = await _seasonRepository.GetByIdWithWeeksAndGamesAsync(seasonId);
            if (season == null)
            {
                return new SeasonSimulationResult { Error = $"Season {seasonId} not found" };
            }

            // Determine which week to revert
            // If season is complete, we revert the last week
            // If season is in progress, we revert CurrentWeek - 1
            int weekToRevertNum;
            
            if (season.IsComplete)
            {
                // Find the last week number
                weekToRevertNum = season.Weeks.Max(w => w.WeekNumber);
            }
            else
            {
                weekToRevertNum = season.CurrentWeek - 1;
            }

            if (weekToRevertNum < 1)
            {
                return new SeasonSimulationResult { Error = "Cannot revert: No previous weeks to revert to." };
            }

            var weekToRevert = season.Weeks.FirstOrDefault(w => w.WeekNumber == weekToRevertNum);
            if (weekToRevert == null)
            {
                return new SeasonSimulationResult { Error = $"Week {weekToRevertNum} not found." };
            }

            if (weekToRevert.Status != WeekStatus.Completed)
            {
                return new SeasonSimulationResult { Error = $"Week {weekToRevertNum} is not completed, cannot revert." };
            }

            _logger.LogInformation("Reverting Season {SeasonId} Week {Week}", seasonId, weekToRevertNum);

            // Reset games
            foreach (var game in weekToRevert.Games)
            {
                // Revert Team Stats (Wins/Losses/Ties)
                if (game.IsComplete)
                {
                    var homeTeam = await _teamRepository.GetByIdAsync(game.HomeTeamId);
                    var awayTeam = await _teamRepository.GetByIdAsync(game.AwayTeamId);

                    if (homeTeam != null && awayTeam != null)
                    {
                        if (game.HomeScore > game.AwayScore)
                        {
                            homeTeam.Wins = Math.Max(0, homeTeam.Wins - 1);
                            awayTeam.Losses = Math.Max(0, awayTeam.Losses - 1);
                        }
                        else if (game.AwayScore > game.HomeScore)
                        {
                            awayTeam.Wins = Math.Max(0, awayTeam.Wins - 1);
                            homeTeam.Losses = Math.Max(0, homeTeam.Losses - 1);
                        }
                        else // Tie
                        {
                            homeTeam.Ties = Math.Max(0, homeTeam.Ties - 1);
                            awayTeam.Ties = Math.Max(0, awayTeam.Ties - 1);
                        }
                        
                        await _teamRepository.UpdateAsync(homeTeam);
                        await _teamRepository.UpdateAsync(awayTeam);
                    }
                }

                game.IsComplete = false;
                game.HomeScore = 0;
                game.AwayScore = 0;
                game.PlayedAt = null;
                game.RandomSeed = null;
                // Note: We might want to clear PlayByPlay data here if it was stored
                
                await _gameRepository.UpdateAsync(game);
            }

            // Reset week status
            weekToRevert.Status = WeekStatus.Scheduled;
            weekToRevert.CompletedDate = null;

            // Move season pointer back
            season.CurrentWeek = weekToRevertNum;
            season.IsComplete = false;
            season.Phase = weekToRevert.Phase;

            await _seasonRepository.UpdateAsync(season);
            await _seasonRepository.SaveChangesAsync();

            return new SeasonSimulationResult
            {
                SeasonId = seasonId,
                WeekNumber = weekToRevertNum,
                GamesSimulated = 0,
                SeasonCompleted = false
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reverting week for season {SeasonId}", seasonId);
            return new SeasonSimulationResult { Error = $"Revert failed: {ex.Message}" };
        }
    }
}

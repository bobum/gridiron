using DataAccessLayer.Repositories;
using DomainObjects;
using DomainObjects.Helpers;
using Microsoft.Extensions.Logging;
using StateLibrary;
using System.Text;
using System.Text.Json;

namespace Gridiron.WebApi.Services;

/// <summary>
/// Service for running game simulations asynchronously
/// DOES NOT access the database directly - uses repositories from DataAccessLayer
/// </summary>
public class GameSimulationService : IGameSimulationService
{
    private readonly ITeamRepository _teamRepository;
    private readonly IGameRepository _gameRepository;
    private readonly IPlayByPlayRepository _playByPlayRepository;
    private readonly ILogger<GameFlow> _gameLogger;
    private readonly ILogger<GameSimulationService> _logger;

    public GameSimulationService(
        ITeamRepository teamRepository,
        IGameRepository gameRepository,
        IPlayByPlayRepository playByPlayRepository,
        ILogger<GameFlow> _gameLogger,
        ILogger<GameSimulationService> logger)
    {
        _teamRepository = teamRepository ?? throw new ArgumentNullException(nameof(teamRepository));
        _gameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository));
        _playByPlayRepository = playByPlayRepository ?? throw new ArgumentNullException(nameof(playByPlayRepository));
        this._gameLogger = _gameLogger ?? throw new ArgumentNullException(nameof(_gameLogger));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Game> SimulateGameAsync(int homeTeamId, int awayTeamId, int? randomSeed = null)
    {
        _logger.LogInformation("Starting game simulation: Home={HomeTeamId}, Away={AwayTeamId}, Seed={Seed}",
            homeTeamId, awayTeamId, randomSeed);

        // Load teams from repository (with all players)
        var homeTeam = await _teamRepository.GetByIdWithPlayersAsync(homeTeamId);
        var awayTeam = await _teamRepository.GetByIdWithPlayersAsync(awayTeamId);

        if (homeTeam == null)
            throw new ArgumentException($"Home team with ID {homeTeamId} not found", nameof(homeTeamId));

        if (awayTeam == null)
            throw new ArgumentException($"Away team with ID {awayTeamId} not found", nameof(awayTeamId));

        // Build depth charts for both teams using the Teams helper
        // This is REQUIRED for the simulation to work - depth charts are used to select players
        var teamsWithDepthCharts = new Teams(homeTeam, awayTeam);

        // Create game
        var game = new Game
        {
            HomeTeam = teamsWithDepthCharts.HomeTeam,
            AwayTeam = teamsWithDepthCharts.VisitorTeam,
            HomeTeamId = homeTeamId,
            AwayTeamId = awayTeamId,
            RandomSeed = randomSeed
        };

        // Create RNG (with seed if provided)
        var rng = randomSeed.HasValue
            ? new SeedableRandom(randomSeed.Value)
            : new SeedableRandom();

        // Run simulation on background thread to avoid blocking
        await Task.Run(() =>
        {
            var gameFlow = new GameFlow(game, rng, _gameLogger);
            gameFlow.Execute();
        });

        _logger.LogInformation("Game simulation completed: {HomeTeam} {HomeScore} - {AwayScore} {AwayTeam}",
            homeTeam.Name, game.HomeScore, game.AwayScore, awayTeam.Name);

        // Save game through repository
        await _gameRepository.AddAsync(game);

        // Create and save play-by-play data
        await SavePlayByPlayDataAsync(game);

        return game;
    }

    /// <summary>
    /// Serializes and saves play-by-play data for a completed game
    /// </summary>
    private async Task SavePlayByPlayDataAsync(Game game)
    {
        try
        {
            // Serialize plays to JSON
            var playsJson = SerializePlays(game.Plays);

            // Generate play-by-play log
            var playByPlayLog = GeneratePlayByPlayLog(game);

            // Create PlayByPlay entity
            var playByPlay = new PlayByPlay
            {
                GameId = game.Id,
                PlaysJson = playsJson,
                PlayByPlayLog = playByPlayLog
            };

            // Save to database
            await _playByPlayRepository.AddAsync(playByPlay);

            _logger.LogInformation("Play-by-play data saved for game {GameId}: {PlayCount} plays, {LogLength} chars",
                game.Id, game.Plays.Count, playByPlayLog.Length);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save play-by-play data for game {GameId}", game.Id);
            // Don't throw - game is already saved, this is supplementary data
        }
    }

    /// <summary>
    /// Serializes the plays list to JSON format
    /// </summary>
    private string SerializePlays(List<IPlay> plays)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };

            return JsonSerializer.Serialize(plays, options);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to serialize plays to JSON, returning basic play summary");

            // Fallback: create a simple JSON array with basic play information
            var playsSummary = plays.Select(p => new
            {
                PlayType = p.PlayType.ToString(),
                Possession = p.Possession.ToString(),
                Down = p.Down.ToString(),
                YardsGained = p.YardsGained,
                StartFieldPosition = p.StartFieldPosition,
                EndFieldPosition = p.EndFieldPosition,
                IsTouchdown = p.IsTouchdown,
                IsSafety = p.IsSafety,
                Interception = p.Interception
            }).ToList();

            return JsonSerializer.Serialize(playsSummary);
        }
    }

    /// <summary>
    /// Generates a human-readable play-by-play log from the game
    /// </summary>
    private string GeneratePlayByPlayLog(Game game)
    {
        var log = new StringBuilder();

        log.AppendLine($"========================================");
        log.AppendLine($"Game: {game.HomeTeam.Name} vs {game.AwayTeam.Name}");
        log.AppendLine($"Final Score: {game.HomeTeam.Name} {game.HomeScore} - {game.AwayScore} {game.AwayTeam.Name}");
        if (game.RandomSeed.HasValue)
        {
            log.AppendLine($"Random Seed: {game.RandomSeed.Value}");
        }
        log.AppendLine($"========================================");
        log.AppendLine();

        // Add play summaries
        for (int i = 0; i < game.Plays.Count; i++)
        {
            var play = game.Plays[i];
            log.AppendLine($"Play {i + 1}: {play.PlayType} - Down: {play.Down}, Possession: {play.Possession}");

            // Add basic play information
            if (play.StartFieldPosition > 0)
            {
                log.AppendLine($"  Field Position: {play.StartFieldPosition} -> {play.EndFieldPosition}");
            }

            if (play.YardsGained != 0)
            {
                log.AppendLine($"  Yards Gained: {play.YardsGained}");
            }

            if (play.IsTouchdown)
            {
                log.AppendLine($"  TOUCHDOWN!");
            }

            if (play.Interception)
            {
                log.AppendLine($"  INTERCEPTION!");
            }

            if (play.Fumbles.Any())
            {
                log.AppendLine($"  FUMBLE! ({play.Fumbles.Count})");
            }

            if (play.Penalties.Any())
            {
                log.AppendLine($"  Penalties: {play.Penalties.Count}");
            }

            if (play.Injuries.Any())
            {
                log.AppendLine($"  Injuries: {play.Injuries.Count}");
            }

            log.AppendLine();
        }

        log.AppendLine($"========================================");
        log.AppendLine($"Total Plays: {game.Plays.Count}");
        log.AppendLine($"========================================");

        return log.ToString();
    }

    public async Task<Game?> GetGameAsync(int gameId)
    {
        return await _gameRepository.GetByIdWithTeamsAsync(gameId);
    }

    public async Task<List<Game>> GetGamesAsync()
    {
        return await _gameRepository.GetAllAsync();
    }
}

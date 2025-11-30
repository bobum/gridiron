using DataAccessLayer.Repositories;
using DomainObjects;
using GameManagement.Services;
using Gridiron.Engine.Simulation;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Gridiron.WebApi.Services;

/// <summary>
/// Service for running game simulations using the Gridiron.Engine NuGet package.
/// Handles the full simulation workflow: load teams, run simulation, persist results.
/// </summary>
public class GameSimulationService : IGameSimulationService
{
    private readonly ITeamRepository _teamRepository;
    private readonly IGameRepository _gameRepository;
    private readonly IPlayByPlayRepository _playByPlayRepository;
    private readonly IEngineSimulationService _engineSimulationService;
    private readonly ILogger<GameSimulationService> _logger;

    public GameSimulationService(
        ITeamRepository teamRepository,
        IGameRepository gameRepository,
        IPlayByPlayRepository playByPlayRepository,
        IEngineSimulationService engineSimulationService,
        ILogger<GameSimulationService> logger)
    {
        _teamRepository = teamRepository ?? throw new ArgumentNullException(nameof(teamRepository));
        _gameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository));
        _playByPlayRepository = playByPlayRepository ?? throw new ArgumentNullException(nameof(playByPlayRepository));
        _engineSimulationService = engineSimulationService ?? throw new ArgumentNullException(nameof(engineSimulationService));
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

        // Create a capture logger to collect play-by-play output during simulation
        var captureLogger = new StringCaptureLogger<GameFlow>();

        // Run simulation using Gridiron.Engine (via EngineSimulationService)
        var result = await Task.Run(() =>
            _engineSimulationService.SimulateGame(homeTeam, awayTeam, randomSeed, captureLogger));

        _logger.LogInformation("Game simulation completed: {HomeTeam} {HomeScore} - {AwayScore} {AwayTeam}",
            homeTeam.Name, result.HomeScore, result.AwayScore, awayTeam.Name);

        // Create Game entity for persistence
        var game = new Game
        {
            HomeTeam = homeTeam,
            AwayTeam = awayTeam,
            HomeTeamId = homeTeamId,
            AwayTeamId = awayTeamId,
            HomeScore = result.HomeScore,
            AwayScore = result.AwayScore,
            RandomSeed = randomSeed
        };

        // Save game through repository
        await _gameRepository.AddAsync(game);

        // Create and save play-by-play data
        await SavePlayByPlayDataAsync(game, result, captureLogger);

        return game;
    }

    /// <summary>
    /// Serializes and saves play-by-play data for a completed game
    /// </summary>
    private async Task SavePlayByPlayDataAsync(Game game, EngineSimulationResult result, StringCaptureLogger<GameFlow> captureLogger)
    {
        try
        {
            // Serialize plays to JSON
            var playsJson = SerializePlays(result.Plays);

            // Get the play-by-play log from the captured logger output
            var playByPlayLog = captureLogger.GetCapturedLog();

            // Create PlayByPlay entity
            var playByPlay = new PlayByPlay
            {
                Game = game,
                GameId = game.Id,
                PlaysJson = playsJson,
                PlayByPlayLog = playByPlayLog
            };

            // Save to database
            await _playByPlayRepository.AddAsync(playByPlay);

            _logger.LogInformation("Play-by-play data saved for game {GameId}: {PlayCount} plays, {LogLength} chars",
                game.Id, result.TotalPlays, playByPlayLog.Length);
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
    private string SerializePlays(IReadOnlyList<Gridiron.Engine.Domain.IPlay> plays)
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

    public async Task<Game?> GetGameAsync(int gameId)
    {
        return await _gameRepository.GetByIdWithTeamsAsync(gameId);
    }

    public async Task<List<Game>> GetGamesAsync()
    {
        return await _gameRepository.GetAllAsync();
    }
}

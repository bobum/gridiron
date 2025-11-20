using DataAccessLayer.Repositories;
using DomainObjects;
using DomainObjects.Helpers;
using Microsoft.Extensions.Logging;
using StateLibrary;

namespace Gridiron.WebApi.Services;

/// <summary>
/// Service for running game simulations asynchronously
/// DOES NOT access the database directly - uses repositories from DataAccessLayer
/// </summary>
public class GameSimulationService : IGameSimulationService
{
    private readonly ITeamRepository _teamRepository;
    private readonly IGameRepository _gameRepository;
    private readonly ILogger<GameFlow> _gameLogger;
    private readonly ILogger<GameSimulationService> _logger;

    public GameSimulationService(
        ITeamRepository teamRepository,
        IGameRepository gameRepository,
        ILogger<GameFlow> gameLogger,
        ILogger<GameSimulationService> logger)
    {
        _teamRepository = teamRepository ?? throw new ArgumentNullException(nameof(teamRepository));
        _gameRepository = gameRepository ?? throw new ArgumentNullException(nameof(gameRepository));
        _gameLogger = gameLogger ?? throw new ArgumentNullException(nameof(gameLogger));
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

        // Create game
        var game = new Game
        {
            HomeTeam = homeTeam,
            AwayTeam = awayTeam,
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

        return game;
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

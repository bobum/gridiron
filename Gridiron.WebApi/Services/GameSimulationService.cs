using DataAccessLayer;
using DomainObjects;
using DomainObjects.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StateLibrary;

namespace Gridiron.WebApi.Services;

/// <summary>
/// Service for running game simulations asynchronously
/// </summary>
public class GameSimulationService : IGameSimulationService
{
    private readonly GridironDbContext _dbContext;
    private readonly ILogger<GameFlow> _gameLogger;
    private readonly ILogger<GameSimulationService> _logger;

    public GameSimulationService(
        GridironDbContext dbContext,
        ILogger<GameFlow> gameLogger,
        ILogger<GameSimulationService> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _gameLogger = gameLogger ?? throw new ArgumentNullException(nameof(gameLogger));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Game> SimulateGameAsync(int homeTeamId, int awayTeamId, int? randomSeed = null)
    {
        _logger.LogInformation("Starting game simulation: Home={HomeTeamId}, Away={AwayTeamId}, Seed={Seed}",
            homeTeamId, awayTeamId, randomSeed);

        // Load teams from database with all players
        var homeTeam = await _dbContext.Teams
            .Include(t => t.Players)
            .FirstOrDefaultAsync(t => t.Id == homeTeamId);

        var awayTeam = await _dbContext.Teams
            .Include(t => t.Players)
            .FirstOrDefaultAsync(t => t.Id == awayTeamId);

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

        // Save game to database
        _dbContext.Games.Add(game);
        await _dbContext.SaveChangesAsync();

        return game;
    }

    public async Task<Game?> GetGameAsync(int gameId)
    {
        return await _dbContext.Games
            .Include(g => g.HomeTeam)
            .Include(g => g.AwayTeam)
            .FirstOrDefaultAsync(g => g.Id == gameId);
    }

    public async Task<List<Game>> GetGamesAsync()
    {
        return await _dbContext.Games
            .Include(g => g.HomeTeam)
            .Include(g => g.AwayTeam)
            .OrderByDescending(g => g.Id)
            .ToListAsync();
    }
}

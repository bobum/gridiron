using Gridiron.Engine.Api;
using Gridiron.Engine.Domain;
using GameManagement.Mapping;
using Microsoft.Extensions.Logging;

namespace GameManagement.Services;

/// <summary>
/// Service that orchestrates game simulation using the Gridiron Engine.
/// Maps between EF entities and engine domain objects.
/// </summary>
public class EngineSimulationService : IEngineSimulationService
{
    private readonly IGameEngine _engine;
    private readonly GridironMapper _mapper;
    private readonly ILogger<EngineSimulationService> _logger;

    public EngineSimulationService(
        IGameEngine engine,
        GridironMapper mapper,
        ILogger<EngineSimulationService> logger)
    {
        _engine = engine;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Simulates a game between two teams.
    /// </summary>
    /// <param name="homeTeam">Home team EF entity</param>
    /// <param name="awayTeam">Away team EF entity</param>
    /// <param name="randomSeed">Optional seed for deterministic simulation</param>
    /// <returns>Simulation result with updated team data</returns>
    public EngineSimulationResult SimulateGame(Team homeTeam, Team awayTeam, int? randomSeed = null)
    {
        _logger.LogInformation(
            "Simulating game: {HomeTeam} vs {AwayTeam} (seed: {Seed})",
            homeTeam.Name, awayTeam.Name, randomSeed?.ToString() ?? "random");

        // Map EF entities to engine domain objects
        var engineHomeTeam = _mapper.ToEngineTeam(homeTeam);
        var engineAwayTeam = _mapper.ToEngineTeam(awayTeam);

        // Run simulation
        var options = new SimulationOptions { RandomSeed = randomSeed };
        var result = _engine.SimulateGame(engineHomeTeam, engineAwayTeam, options);

        _logger.LogInformation(
            "Game completed: {HomeTeam} {HomeScore} - {AwayScore} {AwayTeam} ({TotalPlays} plays)",
            homeTeam.Name, result.HomeScore, result.AwayScore, awayTeam.Name, result.TotalPlays);

        // Update EF entities with simulation results
        _mapper.UpdateTeamEntity(result.HomeTeam, homeTeam);
        _mapper.UpdateTeamEntity(result.AwayTeam, awayTeam);

        // Update player stats
        for (int i = 0; i < homeTeam.Players.Count && i < result.HomeTeam.Players.Count; i++)
        {
            _mapper.UpdatePlayerEntity(result.HomeTeam.Players[i], homeTeam.Players[i]);
        }
        for (int i = 0; i < awayTeam.Players.Count && i < result.AwayTeam.Players.Count; i++)
        {
            _mapper.UpdatePlayerEntity(result.AwayTeam.Players[i], awayTeam.Players[i]);
        }

        return new EngineSimulationResult
        {
            HomeTeam = homeTeam,
            AwayTeam = awayTeam,
            HomeScore = result.HomeScore,
            AwayScore = result.AwayScore,
            TotalPlays = result.TotalPlays,
            IsTie = result.IsTie,
            WinningTeam = result.IsTie ? null : (result.HomeScore > result.AwayScore ? homeTeam : awayTeam),
            RandomSeed = result.RandomSeed,
            Plays = result.Plays
        };
    }

    /// <summary>
    /// Simulates a game with a custom logger for capturing play-by-play output.
    /// </summary>
    public EngineSimulationResult SimulateGame(Team homeTeam, Team awayTeam, int? randomSeed, ILogger? playByPlayLogger)
    {
        _logger.LogInformation(
            "Simulating game: {HomeTeam} vs {AwayTeam} (seed: {Seed})",
            homeTeam.Name, awayTeam.Name, randomSeed?.ToString() ?? "random");

        // Map EF entities to engine domain objects
        var engineHomeTeam = _mapper.ToEngineTeam(homeTeam);
        var engineAwayTeam = _mapper.ToEngineTeam(awayTeam);

        // Run simulation with custom logger for play-by-play capture
        var options = new SimulationOptions 
        { 
            RandomSeed = randomSeed,
            Logger = playByPlayLogger
        };
        var result = _engine.SimulateGame(engineHomeTeam, engineAwayTeam, options);

        _logger.LogInformation(
            "Game completed: {HomeTeam} {HomeScore} - {AwayScore} {AwayTeam} ({TotalPlays} plays)",
            homeTeam.Name, result.HomeScore, result.AwayScore, awayTeam.Name, result.TotalPlays);

        // Update EF entities with simulation results
        _mapper.UpdateTeamEntity(result.HomeTeam, homeTeam);
        _mapper.UpdateTeamEntity(result.AwayTeam, awayTeam);

        // Update player stats
        for (int i = 0; i < homeTeam.Players.Count && i < result.HomeTeam.Players.Count; i++)
        {
            _mapper.UpdatePlayerEntity(result.HomeTeam.Players[i], homeTeam.Players[i]);
        }
        for (int i = 0; i < awayTeam.Players.Count && i < result.AwayTeam.Players.Count; i++)
        {
            _mapper.UpdatePlayerEntity(result.AwayTeam.Players[i], awayTeam.Players[i]);
        }

        return new EngineSimulationResult
        {
            HomeTeam = homeTeam,
            AwayTeam = awayTeam,
            HomeScore = result.HomeScore,
            AwayScore = result.AwayScore,
            TotalPlays = result.TotalPlays,
            IsTie = result.IsTie,
            WinningTeam = result.IsTie ? null : (result.HomeScore > result.AwayScore ? homeTeam : awayTeam),
            RandomSeed = result.RandomSeed,
            Plays = result.Plays
        };
    }
}

/// <summary>
/// Interface for engine-level game simulation service.
/// Takes domain objects directly and handles mapping to/from engine types.
/// </summary>
public interface IEngineSimulationService
{
    EngineSimulationResult SimulateGame(Team homeTeam, Team awayTeam, int? randomSeed = null);
    
    /// <summary>
    /// Simulates a game with a custom logger for capturing play-by-play output.
    /// </summary>
    EngineSimulationResult SimulateGame(Team homeTeam, Team awayTeam, int? randomSeed, ILogger? playByPlayLogger);
}

/// <summary>
/// Result of an engine simulation.
/// </summary>
public class EngineSimulationResult
{
    public required Team HomeTeam { get; init; }
    public required Team AwayTeam { get; init; }
    public int HomeScore { get; init; }
    public int AwayScore { get; init; }
    public int TotalPlays { get; init; }
    public bool IsTie { get; init; }
    public Team? WinningTeam { get; init; }
    public int? RandomSeed { get; init; }
    
    /// <summary>
    /// All plays from the simulated game (for play-by-play serialization).
    /// </summary>
    public IReadOnlyList<IPlay> Plays { get; init; } = Array.Empty<IPlay>();
}

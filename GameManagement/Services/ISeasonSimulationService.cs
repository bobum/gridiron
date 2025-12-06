using DomainObjects;

namespace GameManagement.Services;

/// <summary>
/// Service for managing season simulation and week advancement.
/// </summary>
public interface ISeasonSimulationService
{
    /// <summary>
    /// Simulates all unplayed games in the current week of the specified season and advances the week.
    /// </summary>
    /// <param name="seasonId">The ID of the season to simulate.</param>
    /// <returns>A result object containing details about the simulation.</returns>
    Task<SeasonSimulationResult> SimulateCurrentWeekAsync(int seasonId);

    /// <summary>
    /// Reverts the last completed week, resetting game results and moving the season pointer back.
    /// </summary>
    /// <param name="seasonId">The ID of the season to revert.</param>
    /// <returns>A result object indicating success or failure.</returns>
    Task<SeasonSimulationResult> RevertLastWeekAsync(int seasonId);
}

/// <summary>
/// Result of a season week simulation.
/// </summary>
public class SeasonSimulationResult
{
    public int SeasonId { get; set; }
    public int WeekNumber { get; set; }
    public int GamesSimulated { get; set; }
    public List<GameSimulationResult> GameResults { get; set; } = new();
    public bool SeasonCompleted { get; set; }
    public string? Error { get; set; }
    public bool Success => string.IsNullOrEmpty(Error);
}

/// <summary>
/// Result of a single game simulation within a season week.
/// </summary>
public class GameSimulationResult
{
    public int GameId { get; set; }
    public string HomeTeam { get; set; } = string.Empty;
    public string AwayTeam { get; set; } = string.Empty;
    public int HomeScore { get; set; }
    public int AwayScore { get; set; }
    public bool IsTie { get; set; }
}

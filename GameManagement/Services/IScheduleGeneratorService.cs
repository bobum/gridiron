using DomainObjects;

namespace GameManagement.Services;

/// <summary>
/// Service for generating NFL-style regular season schedules.
/// Creates 17-week schedules with division matchups, conference matchups,
/// inter-conference games, and bye weeks.
/// </summary>
public interface IScheduleGeneratorService
{
    /// <summary>
    /// Generates a complete regular season schedule for a league.
    /// Creates SeasonWeeks with scheduled Games for all teams.
    /// </summary>
    /// <param name="season">The season to generate schedule for (must have League loaded with full structure)</param>
    /// <param name="seed">Optional seed for reproducible schedule generation</param>
    /// <returns>The season with populated Weeks and Games</returns>
    /// <remarks>
    /// NFL-style schedule:
    /// - 6 division games (2x each division rival)
    /// - 4 games vs another division in same conference (rotating yearly)
    /// - 4 games vs a division in other conference (rotating yearly)
    /// - 2 games vs same-place finishers from remaining divisions (not implemented - uses random)
    /// - 1 bye week per team
    /// Total: 17 games per team over 18 weeks
    /// </remarks>
    Season GenerateSchedule(Season season, int? seed = null);

    /// <summary>
    /// Validates that a league structure can support NFL-style scheduling.
    /// Requires 2 conferences with equal divisions and 4 teams per division.
    /// </summary>
    /// <param name="league">The league to validate</param>
    /// <returns>Validation result with any errors</returns>
    ScheduleValidationResult ValidateLeagueStructure(League league);
}

/// <summary>
/// Result of validating a league structure for schedule generation
/// </summary>
public class ScheduleValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();

    public static ScheduleValidationResult Success() => new() { IsValid = true };

    public static ScheduleValidationResult Failure(params string[] errors) => new()
    {
        IsValid = false,
        Errors = errors.ToList()
    };
}

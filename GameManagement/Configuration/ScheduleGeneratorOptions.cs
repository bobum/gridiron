namespace GameManagement.Configuration;

/// <summary>
/// Configuration options for the schedule generator service.
/// These values represent NFL-style hard caps and can be adjusted via appsettings.json.
/// </summary>
public class ScheduleGeneratorOptions
{
    /// <summary>
    /// Configuration section name in appsettings.json
    /// </summary>
    public const string SectionName = "ScheduleGenerator";

    /// <summary>
    /// Maximum number of conferences allowed in a league.
    /// NFL default: 2 (AFC and NFC)
    /// </summary>
    public int MaxConferences { get; set; } = 2;

    /// <summary>
    /// Maximum number of divisions allowed per conference.
    /// NFL default: 4 (North, South, East, West)
    /// </summary>
    public int MaxDivisionsPerConference { get; set; } = 4;

    /// <summary>
    /// Maximum number of teams allowed per division.
    /// NFL default: 4
    /// </summary>
    public int MaxTeamsPerDivision { get; set; } = 4;

    /// <summary>
    /// Maximum total number of teams allowed in a league.
    /// NFL default: 32 (2 conferences * 4 divisions * 4 teams)
    /// </summary>
    public int MaxTotalTeams { get; set; } = 32;

    /// <summary>
    /// Maximum number of regular season weeks allowed.
    /// NFL default: 18
    /// </summary>
    public int MaxRegularSeasonWeeks { get; set; } = 18;

    /// <summary>
    /// Default number of regular season weeks when creating a new season.
    /// NFL default: 17
    /// </summary>
    public int DefaultRegularSeasonWeeks { get; set; } = 17;
}

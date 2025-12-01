namespace GameManagement.Configuration;

/// <summary>
/// Constants for schedule generation following NFL-style rules.
/// All hard-coded values for league structure and schedule limits are centralized here.
/// </summary>
public static class ScheduleConstants
{
    /// <summary>
    /// Maximum number of conferences allowed in a league.
    /// NFL: 2 (AFC and NFC)
    /// </summary>
    public const int MaxConferences = 2;

    /// <summary>
    /// Maximum number of divisions allowed per conference.
    /// NFL: 4 (North, South, East, West)
    /// </summary>
    public const int MaxDivisionsPerConference = 4;

    /// <summary>
    /// Maximum number of teams allowed per division.
    /// NFL: 4
    /// </summary>
    public const int MaxTeamsPerDivision = 4;

    /// <summary>
    /// Maximum total number of teams allowed in a league.
    /// NFL: 32 (2 conferences * 4 divisions * 4 teams)
    /// </summary>
    public const int MaxTotalTeams = MaxConferences * MaxDivisionsPerConference * MaxTeamsPerDivision;

    /// <summary>
    /// Maximum number of regular season weeks allowed.
    /// NFL: 18
    /// </summary>
    public const int MaxRegularSeasonWeeks = 18;

    /// <summary>
    /// Default number of regular season weeks when creating a new season.
    /// NFL: 17
    /// </summary>
    public const int DefaultRegularSeasonWeeks = 17;
}

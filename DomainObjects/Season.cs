namespace DomainObjects;

/// <summary>
/// Represents a season within a league, tracking progression through weeks and phases.
/// </summary>
public class Season : SoftDeletableEntity
{
    /// <summary>
    /// Primary key
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to the league this season belongs to
    /// </summary>
    public int LeagueId { get; set; }

    /// <summary>
    /// Navigation property to the league
    /// </summary>
    public League League { get; set; } = null!;

    /// <summary>
    /// The year this season represents (e.g., 2024)
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Current week number within the current phase (1-based)
    /// For regular season: 1-17 (or configured number of weeks)
    /// For playoffs: 1-4 (Wild Card, Divisional, Conference, Super Bowl)
    /// </summary>
    public int CurrentWeek { get; set; } = 1;

    /// <summary>
    /// Current phase of the season
    /// </summary>
    public SeasonPhase Phase { get; set; } = SeasonPhase.Preseason;

    /// <summary>
    /// Whether this season has been completed
    /// </summary>
    public bool IsComplete { get; set; } = false;

    /// <summary>
    /// Date when this season started
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Date when this season ended (set when IsComplete = true)
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Number of regular season weeks configured for this season
    /// </summary>
    public int RegularSeasonWeeks { get; set; } = 17;

    /// <summary>
    /// The team that won the championship (set after playoffs complete)
    /// </summary>
    public int? ChampionTeamId { get; set; }

    /// <summary>
    /// Navigation property to the champion team
    /// </summary>
    public Team? ChampionTeam { get; set; }

    /// <summary>
    /// Collection of weeks in this season
    /// </summary>
    public List<SeasonWeek> Weeks { get; set; } = new();

    /// <summary>
    /// Teams that qualified for playoffs (set when playoffs begin)
    /// Stored as JSON array of team IDs
    /// </summary>
    public string? PlayoffTeamIds { get; set; }

    /// <summary>
    /// Advances the season to the next week within the current phase.
    /// Returns false if at the end of the phase.
    /// </summary>
    public bool AdvanceWeek()
    {
        if (IsComplete) return false;

        switch (Phase)
        {
            case SeasonPhase.Preseason:
                if (CurrentWeek < 4) // Typically 4 preseason weeks
                {
                    CurrentWeek++;
                    return true;
                }
                return false;

            case SeasonPhase.RegularSeason:
                if (CurrentWeek < RegularSeasonWeeks)
                {
                    CurrentWeek++;
                    return true;
                }
                return false;

            case SeasonPhase.Playoffs:
                if (CurrentWeek < 4) // Wild Card, Divisional, Conference, Championship
                {
                    CurrentWeek++;
                    return true;
                }
                return false;

            default:
                return false;
        }
    }

    /// <summary>
    /// Advances to the next phase of the season.
    /// </summary>
    public bool AdvancePhase()
    {
        if (IsComplete) return false;

        switch (Phase)
        {
            case SeasonPhase.Preseason:
                Phase = SeasonPhase.RegularSeason;
                CurrentWeek = 1;
                return true;

            case SeasonPhase.RegularSeason:
                Phase = SeasonPhase.Playoffs;
                CurrentWeek = 1;
                return true;

            case SeasonPhase.Playoffs:
                Phase = SeasonPhase.Offseason;
                CurrentWeek = 1;
                IsComplete = true;
                EndDate = DateTime.UtcNow;
                return true;

            default:
                return false;
        }
    }
}

namespace DomainObjects;

/// <summary>
/// Represents a week within a season, containing scheduled games.
/// </summary>
public class SeasonWeek : SoftDeletableEntity
{
    /// <summary>
    /// Gets or sets primary key.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets foreign key to the season this week belongs to.
    /// </summary>
    public int SeasonId { get; set; }

    /// <summary>
    /// Gets or sets navigation property to the season.
    /// </summary>
    public Season Season { get; set; } = null!;

    /// <summary>
    /// Gets or sets week number within the phase (1-based).
    /// </summary>
    public int WeekNumber { get; set; }

    /// <summary>
    /// Gets or sets which phase of the season this week belongs to.
    /// </summary>
    public SeasonPhase Phase { get; set; }

    /// <summary>
    /// Gets or sets status of this week.
    /// </summary>
    public WeekStatus Status { get; set; } = WeekStatus.Scheduled;

    /// <summary>
    /// Gets or sets scheduled start date for this week's games.
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Gets or sets date when all games in this week were completed.
    /// </summary>
    public DateTime? CompletedDate { get; set; }

    /// <summary>
    /// Gets or sets games scheduled for this week.
    /// </summary>
    public List<Game> Games { get; set; } = new ();

    /// <summary>
    /// Gets display name for the week (e.g., "Week 1", "Wild Card Round", "Super Bowl").
    /// </summary>
    public string DisplayName
    {
        get
        {
            return Phase switch
            {
                SeasonPhase.Preseason => $"Preseason Week {WeekNumber}",
                SeasonPhase.RegularSeason => $"Week {WeekNumber}",
                SeasonPhase.Playoffs => WeekNumber switch
                {
                    1 => "Wild Card Round",
                    2 => "Divisional Round",
                    3 => "Conference Championships",
                    4 => "Championship Game",
                    _ => $"Playoff Round {WeekNumber}"
                },
                SeasonPhase.Offseason => "Offseason",
                _ => $"Week {WeekNumber}"
            };
        }
    }

    /// <summary>
    /// Gets a value indicating whether whether all games in this week have been completed.
    /// </summary>
    public bool IsComplete => Status == WeekStatus.Completed;
}

/// <summary>
/// Status of a season week.
/// </summary>
public enum WeekStatus
{
    /// <summary>
    /// Week is scheduled but games haven't started
    /// </summary>
    Scheduled,

    /// <summary>
    /// Games are currently being played
    /// </summary>
    InProgress,

    /// <summary>
    /// All games have been completed
    /// </summary>
    Completed
}

using System.ComponentModel.DataAnnotations;

namespace DomainObjects;

/// <summary>
/// Represents a season within a league, tracking progression through weeks and phases.
/// </summary>
public class Season : SoftDeletableEntity
{
    /// <summary>
    /// Gets or sets primary key.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Concurrency token for optimistic locking.
    /// </summary>
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    /// <summary>
    /// Gets or sets foreign key to the league this season belongs to.
    /// </summary>
    public int LeagueId { get; set; }

    /// <summary>
    /// Gets or sets navigation property to the league.
    /// </summary>
    public League League { get; set; } = null!;

    /// <summary>
    /// Gets or sets the year this season represents (e.g., 2024).
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Gets or sets current week number within the current phase (1-based)
    /// For regular season: 1-17 (or configured number of weeks)
    /// For playoffs: 1-4 (Wild Card, Divisional, Conference, Super Bowl).
    /// </summary>
    public int CurrentWeek { get; set; } = 1;

    /// <summary>
    /// Gets or sets current phase of the season.
    /// </summary>
    public SeasonPhase Phase { get; set; } = SeasonPhase.Preseason;

    /// <summary>
    /// Gets or sets a value indicating whether whether this season has been completed.
    /// </summary>
    public bool IsComplete { get; set; } = false;

    /// <summary>
    /// Gets or sets date when this season started.
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Gets or sets date when this season ended (set when IsComplete = true).
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Gets or sets number of regular season weeks configured for this season.
    /// </summary>
    public int RegularSeasonWeeks { get; set; } = 17;

    /// <summary>
    /// Gets or sets the team that won the championship (set after playoffs complete).
    /// </summary>
    public int? ChampionTeamId { get; set; }

    /// <summary>
    /// Gets or sets navigation property to the champion team.
    /// </summary>
    public Team? ChampionTeam { get; set; }

    /// <summary>
    /// Gets or sets collection of weeks in this season.
    /// </summary>
    public List<SeasonWeek> Weeks { get; set; } = new ();

    /// <summary>
    /// Gets or sets teams that qualified for playoffs (set when playoffs begin)
    /// Stored as JSON array of team IDs.
    /// </summary>
    public string? PlayoffTeamIds { get; set; }

    /// <summary>
    /// Advances the season to the next week within the current phase.
    /// Returns false if at the end of the phase.
    /// </summary>
    /// <returns></returns>
    public bool AdvanceWeek()
    {
        if (IsComplete)
        {
            return false;
        }

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
    /// <returns></returns>
    public bool AdvancePhase()
    {
        if (IsComplete)
        {
            return false;
        }

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

namespace DomainObjects;

/// <summary>
/// Junction table that assigns users to roles within specific leagues/teams.
/// A user can have different roles in different leagues.
/// Example: Bob is Commissioner of "NFL 2025" and GM of "Cowboys" in "Fantasy League 2025"
/// </summary>
public class UserLeagueRole : SoftDeletableEntity
{
    /// <summary>
    /// Primary key for EF Core
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The user being assigned a role
    /// </summary>
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    /// <summary>
    /// The league this role applies to
    /// </summary>
    public int LeagueId { get; set; }
    public League League { get; set; } = null!;

    /// <summary>
    /// The role this user has in the league (Commissioner or GM)
    /// </summary>
    public UserRole Role { get; set; }

    /// <summary>
    /// If Role is GeneralManager, this is the specific team they manage.
    /// If Role is Commissioner, this is null (they have access to all teams).
    /// </summary>
    public int? TeamId { get; set; }
    public Team? Team { get; set; }

    /// <summary>
    /// When this role was assigned
    /// </summary>
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
}

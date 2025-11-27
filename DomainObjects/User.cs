namespace DomainObjects;

/// <summary>
/// Represents an authenticated user in the system.
/// Synchronized with Azure Entra ID (External CIAM).
/// </summary>
public class User : SoftDeletableEntity
{
    /// <summary>
    /// Primary key for EF Core
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Azure Entra ID Object ID (oid claim from JWT token)
    /// This is the unique identifier from Azure AD
    /// </summary>
    public string AzureAdObjectId { get; set; } = string.Empty;

    /// <summary>
    /// User's email address from Azure AD
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's display name from Azure AD
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// When the user first authenticated with the system
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last time the user authenticated
    /// </summary>
    public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Is this user a global administrator (God role)?
    /// </summary>
    public bool IsGlobalAdmin { get; set; } = false;

    /// <summary>
    /// Navigation property - roles this user has in various leagues
    /// </summary>
    public List<UserLeagueRole> LeagueRoles { get; set; } = new();
}

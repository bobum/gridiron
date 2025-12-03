namespace DomainObjects;

/// <summary>
/// Represents an authenticated user in the system.
/// Synchronized with Azure Entra ID (External CIAM).
/// </summary>
public class User : SoftDeletableEntity
{
    /// <summary>
    /// Gets or sets primary key for EF Core.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets azure Entra ID Object ID (oid claim from JWT token)
    /// This is the unique identifier from Azure AD.
    /// </summary>
    public string AzureAdObjectId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets user's email address from Azure AD.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets user's display name from Azure AD.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets when the user first authenticated with the system.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets last time the user authenticated.
    /// </summary>
    public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets a value indicating whether is this user a global administrator (God role)?.
    /// </summary>
    public bool IsGlobalAdmin { get; set; } = false;

    /// <summary>
    /// Gets or sets navigation property - roles this user has in various leagues.
    /// </summary>
    public List<UserLeagueRole> LeagueRoles { get; set; } = new ();
}

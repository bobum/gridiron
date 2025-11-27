using DomainObjects;

namespace Gridiron.WebApi.Services;

/// <summary>
/// Service for checking user authorization across leagues and teams.
/// Implements hierarchical role-based access control:
/// - God (Global Admin): Can access everything
/// - Commissioner: Can access all teams/data within their league(s)
/// - General Manager (GM): Can access only their specific team(s)
/// </summary>
public interface IGridironAuthorizationService
{
    /// <summary>
    /// Gets or creates a user from Azure AD claims (call this on every authenticated request)
    /// </summary>
    Task<User> GetOrCreateUserFromClaimsAsync(string azureAdObjectId, string email, string displayName);

    /// <summary>
    /// Checks if the current user can access a specific league
    /// </summary>
    Task<bool> CanAccessLeagueAsync(string azureAdObjectId, int leagueId);

    /// <summary>
    /// Checks if the current user can access a specific team
    /// </summary>
    Task<bool> CanAccessTeamAsync(string azureAdObjectId, int teamId);

    /// <summary>
    /// Checks if the current user is a commissioner of a specific league
    /// </summary>
    Task<bool> IsCommissionerOfLeagueAsync(string azureAdObjectId, int leagueId);

    /// <summary>
    /// Checks if the current user is a GM of a specific team
    /// </summary>
    Task<bool> IsGeneralManagerOfTeamAsync(string azureAdObjectId, int teamId);

    /// <summary>
    /// Checks if the current user is a global administrator (God role)
    /// </summary>
    Task<bool> IsGlobalAdminAsync(string azureAdObjectId);

    /// <summary>
    /// Gets all league IDs the user has access to
    /// </summary>
    Task<List<int>> GetAccessibleLeagueIdsAsync(string azureAdObjectId);

    /// <summary>
    /// Gets all team IDs the user has access to
    /// </summary>
    Task<List<int>> GetAccessibleTeamIdsAsync(string azureAdObjectId);
}

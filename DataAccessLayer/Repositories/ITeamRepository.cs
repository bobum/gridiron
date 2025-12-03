using DomainObjects;

namespace DataAccessLayer.Repositories;

/// <summary>
/// Repository interface for Team data access
/// This is the ONLY way to access team data from the database.
/// </summary>
public interface ITeamRepository
{
    /// <summary>
    /// Gets all teams.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<List<Team>> GetAllAsync();

    /// <summary>
    /// Gets a team by ID.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<Team?> GetByIdAsync(int teamId);

    /// <summary>
    /// Gets a team by ID with all players loaded.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<Team?> GetByIdWithPlayersAsync(int teamId);

    /// <summary>
    /// Gets a team by city and name.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<Team?> GetByCityAndNameAsync(string city, string name);

    /// <summary>
    /// Gets all teams in a specific league.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<List<Team>> GetTeamsByLeagueIdAsync(int leagueId);

    /// <summary>
    /// Adds a new team.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<Team> AddAsync(Team team);

    /// <summary>
    /// Updates an existing team.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task UpdateAsync(Team team);

    /// <summary>
    /// Deletes a team.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task DeleteAsync(int teamId);

    /// <summary>
    /// Soft deletes a team by marking it as deleted.
    /// </summary>
    /// <param name="teamId">ID of the team to soft delete.</param>
    /// <param name="deletedBy">Username or identifier of who is deleting.</param>
    /// <param name="reason">Optional reason for deletion.</param>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task SoftDeleteAsync(int teamId, string? deletedBy = null, string? reason = null);

    /// <summary>
    /// Restores a soft-deleted team.
    /// </summary>
    /// <param name="teamId">ID of the team to restore.</param>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task RestoreAsync(int teamId);

    /// <summary>
    /// Gets all soft-deleted teams.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<List<Team>> GetDeletedAsync();

    /// <summary>
    /// Saves all pending changes.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<int> SaveChangesAsync();
}

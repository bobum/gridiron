using DomainObjects;

namespace DataAccessLayer.Repositories;

/// <summary>
/// Repository interface for Team data access
/// This is the ONLY way to access team data from the database
/// </summary>
public interface ITeamRepository
{
    /// <summary>
    /// Gets all teams
    /// </summary>
    Task<List<Team>> GetAllAsync();

    /// <summary>
    /// Gets a team by ID
    /// </summary>
    Task<Team?> GetByIdAsync(int teamId);

    /// <summary>
    /// Gets a team by ID with all players loaded
    /// </summary>
    Task<Team?> GetByIdWithPlayersAsync(int teamId);

    /// <summary>
    /// Gets a team by city and name
    /// </summary>
    Task<Team?> GetByCityAndNameAsync(string city, string name);

    /// <summary>
    /// Adds a new team
    /// </summary>
    Task<Team> AddAsync(Team team);

    /// <summary>
    /// Updates an existing team
    /// </summary>
    Task UpdateAsync(Team team);

    /// <summary>
    /// Deletes a team
    /// </summary>
    Task DeleteAsync(int teamId);

    /// <summary>
    /// Soft deletes a team by marking it as deleted
    /// </summary>
    /// <param name="teamId">ID of the team to soft delete</param>
    /// <param name="deletedBy">Username or identifier of who is deleting</param>
    /// <param name="reason">Optional reason for deletion</param>
    Task SoftDeleteAsync(int teamId, string? deletedBy = null, string? reason = null);

    /// <summary>
    /// Restores a soft-deleted team
    /// </summary>
    /// <param name="teamId">ID of the team to restore</param>
    Task RestoreAsync(int teamId);

    /// <summary>
    /// Gets all soft-deleted teams
    /// </summary>
    Task<List<Team>> GetDeletedAsync();

    /// <summary>
    /// Saves all pending changes
    /// </summary>
    Task<int> SaveChangesAsync();
}

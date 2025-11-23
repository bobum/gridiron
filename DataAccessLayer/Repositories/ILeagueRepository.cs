using DomainObjects;

namespace DataAccessLayer.Repositories;

/// <summary>
/// Repository interface for League data access
/// This is the ONLY way to access league data from the database
/// </summary>
public interface ILeagueRepository
{
    /// <summary>
    /// Gets all leagues
    /// </summary>
    Task<List<League>> GetAllAsync();

    /// <summary>
    /// Gets a league by ID
    /// </summary>
    Task<League?> GetByIdAsync(int leagueId);

    /// <summary>
    /// Gets a league by ID with full structure (conferences, divisions, teams)
    /// </summary>
    Task<League?> GetByIdWithFullStructureAsync(int leagueId);

    /// <summary>
    /// Gets a league by name
    /// </summary>
    Task<League?> GetByNameAsync(string name);

    /// <summary>
    /// Adds a new league
    /// </summary>
    Task<League> AddAsync(League league);

    /// <summary>
    /// Updates an existing league
    /// </summary>
    Task UpdateAsync(League league);

    /// <summary>
    /// Deletes a league
    /// </summary>
    Task DeleteAsync(int leagueId);

    /// <summary>
    /// Soft deletes a league by marking it as deleted
    /// </summary>
    /// <param name="leagueId">ID of the league to soft delete</param>
    /// <param name="deletedBy">Username or identifier of who is deleting</param>
    /// <param name="reason">Optional reason for deletion</param>
    Task SoftDeleteAsync(int leagueId, string? deletedBy = null, string? reason = null);

    /// <summary>
    /// Restores a soft-deleted league
    /// </summary>
    /// <param name="leagueId">ID of the league to restore</param>
    Task RestoreAsync(int leagueId);

    /// <summary>
    /// Gets all soft-deleted leagues
    /// </summary>
    Task<List<League>> GetDeletedAsync();

    /// <summary>
    /// Saves all pending changes
    /// </summary>
    Task<int> SaveChangesAsync();
}

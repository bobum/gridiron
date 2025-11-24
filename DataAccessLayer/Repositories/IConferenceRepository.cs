using DomainObjects;

namespace DataAccessLayer.Repositories;

/// <summary>
/// Repository interface for Conference data access
/// This is the ONLY way to access conference data from the database
/// </summary>
public interface IConferenceRepository
{
    /// <summary>
    /// Gets all conferences
    /// </summary>
    Task<List<Conference>> GetAllAsync();

    /// <summary>
    /// Gets a conference by ID
    /// </summary>
    Task<Conference?> GetByIdAsync(int conferenceId);

    /// <summary>
    /// Gets a conference by ID with divisions and teams
    /// </summary>
    Task<Conference?> GetByIdWithDivisionsAsync(int conferenceId);

    /// <summary>
    /// Adds a new conference
    /// </summary>
    Task<Conference> AddAsync(Conference conference);

    /// <summary>
    /// Updates an existing conference
    /// </summary>
    Task UpdateAsync(Conference conference);

    /// <summary>
    /// Deletes a conference
    /// </summary>
    Task DeleteAsync(int conferenceId);

    /// <summary>
    /// Soft deletes a conference by marking it as deleted
    /// </summary>
    /// <param name="conferenceId">ID of the conference to soft delete</param>
    /// <param name="deletedBy">Username or identifier of who is deleting</param>
    /// <param name="reason">Optional reason for deletion</param>
    Task SoftDeleteAsync(int conferenceId, string? deletedBy = null, string? reason = null);

    /// <summary>
    /// Restores a soft-deleted conference
    /// </summary>
    /// <param name="conferenceId">ID of the conference to restore</param>
    Task RestoreAsync(int conferenceId);

    /// <summary>
    /// Gets all soft-deleted conferences
    /// </summary>
    Task<List<Conference>> GetDeletedAsync();

    /// <summary>
    /// Saves all pending changes
    /// </summary>
    Task<int> SaveChangesAsync();
}

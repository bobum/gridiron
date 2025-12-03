using DomainObjects;

namespace DataAccessLayer.Repositories;

/// <summary>
/// Repository interface for Conference data access
/// This is the ONLY way to access conference data from the database.
/// </summary>
public interface IConferenceRepository
{
    /// <summary>
    /// Gets all conferences.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<List<Conference>> GetAllAsync();

    /// <summary>
    /// Gets a conference by ID.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<Conference?> GetByIdAsync(int conferenceId);

    /// <summary>
    /// Gets a conference by ID with divisions and teams.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<Conference?> GetByIdWithDivisionsAsync(int conferenceId);

    /// <summary>
    /// Adds a new conference.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<Conference> AddAsync(Conference conference);

    /// <summary>
    /// Updates an existing conference.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task UpdateAsync(Conference conference);

    /// <summary>
    /// Deletes a conference.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task DeleteAsync(int conferenceId);

    /// <summary>
    /// Soft deletes a conference by marking it as deleted.
    /// </summary>
    /// <param name="conferenceId">ID of the conference to soft delete.</param>
    /// <param name="deletedBy">Username or identifier of who is deleting.</param>
    /// <param name="reason">Optional reason for deletion.</param>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task SoftDeleteAsync(int conferenceId, string? deletedBy = null, string? reason = null);

    /// <summary>
    /// Restores a soft-deleted conference.
    /// </summary>
    /// <param name="conferenceId">ID of the conference to restore.</param>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task RestoreAsync(int conferenceId);

    /// <summary>
    /// Gets all soft-deleted conferences.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<List<Conference>> GetDeletedAsync();

    /// <summary>
    /// Saves all pending changes.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<int> SaveChangesAsync();
}

using DomainObjects;

namespace DataAccessLayer.Repositories;

/// <summary>
/// Repository interface for PlayByPlay data access.
/// Manages play-by-play game logs and serialized play data.
/// </summary>
public interface IPlayByPlayRepository
{
    /// <summary>
    /// Get play-by-play data for a specific game.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<PlayByPlay?> GetByGameIdAsync(int gameId);

    /// <summary>
    /// Get play-by-play data by its ID.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<PlayByPlay?> GetByIdAsync(int id);

    /// <summary>
    /// Add new play-by-play record.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<PlayByPlay> AddAsync(PlayByPlay playByPlay);

    /// <summary>
    /// Update existing play-by-play record.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task UpdateAsync(PlayByPlay playByPlay);

    /// <summary>
    /// Delete play-by-play record.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task DeleteAsync(int id);

    /// <summary>
    /// Soft deletes a play-by-play record by marking it as deleted.
    /// </summary>
    /// <param name="id">ID of the play-by-play to soft delete.</param>
    /// <param name="deletedBy">Username or identifier of who is deleting.</param>
    /// <param name="reason">Optional reason for deletion.</param>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task SoftDeleteAsync(int id, string? deletedBy = null, string? reason = null);

    /// <summary>
    /// Restores a soft-deleted play-by-play record.
    /// </summary>
    /// <param name="id">ID of the play-by-play to restore.</param>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task RestoreAsync(int id);

    /// <summary>
    /// Gets all soft-deleted play-by-play records.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<List<PlayByPlay>> GetDeletedAsync();

    /// <summary>
    /// Check if play-by-play exists for a game.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<bool> ExistsForGameAsync(int gameId);

    /// <summary>
    /// Saves all pending changes.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<int> SaveChangesAsync();
}

using DomainObjects;

namespace DataAccessLayer.Repositories;

/// <summary>
/// Repository interface for PlayByPlay data access
/// Manages play-by-play game logs and serialized play data
/// </summary>
public interface IPlayByPlayRepository
{
    /// <summary>
    /// Get play-by-play data for a specific game
    /// </summary>
    Task<PlayByPlay?> GetByGameIdAsync(int gameId);

    /// <summary>
    /// Get play-by-play data by its ID
    /// </summary>
    Task<PlayByPlay?> GetByIdAsync(int id);

    /// <summary>
    /// Add new play-by-play record
    /// </summary>
    Task<PlayByPlay> AddAsync(PlayByPlay playByPlay);

    /// <summary>
    /// Update existing play-by-play record
    /// </summary>
    Task UpdateAsync(PlayByPlay playByPlay);

    /// <summary>
    /// Delete play-by-play record
    /// </summary>
    Task DeleteAsync(int id);

    /// <summary>
    /// Soft deletes a play-by-play record by marking it as deleted
    /// </summary>
    /// <param name="id">ID of the play-by-play to soft delete</param>
    /// <param name="deletedBy">Username or identifier of who is deleting</param>
    /// <param name="reason">Optional reason for deletion</param>
    Task SoftDeleteAsync(int id, string? deletedBy = null, string? reason = null);

    /// <summary>
    /// Restores a soft-deleted play-by-play record
    /// </summary>
    /// <param name="id">ID of the play-by-play to restore</param>
    Task RestoreAsync(int id);

    /// <summary>
    /// Gets all soft-deleted play-by-play records
    /// </summary>
    Task<List<PlayByPlay>> GetDeletedAsync();

    /// <summary>
    /// Check if play-by-play exists for a game
    /// </summary>
    Task<bool> ExistsForGameAsync(int gameId);

    /// <summary>
    /// Saves all pending changes
    /// </summary>
    Task<int> SaveChangesAsync();
}

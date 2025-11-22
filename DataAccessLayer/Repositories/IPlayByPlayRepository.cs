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
    /// Check if play-by-play exists for a game
    /// </summary>
    Task<bool> ExistsForGameAsync(int gameId);
}

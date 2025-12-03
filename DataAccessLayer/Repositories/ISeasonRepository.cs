using DomainObjects;

namespace DataAccessLayer.Repositories;

/// <summary>
/// Repository interface for Season data access
/// This is the ONLY way to access season data from the database.
/// </summary>
public interface ISeasonRepository
{
    /// <summary>
    /// Gets all seasons.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<List<Season>> GetAllAsync();

    /// <summary>
    /// Gets a season by ID.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<Season?> GetByIdAsync(int seasonId);

    /// <summary>
    /// Gets a season by ID with weeks loaded.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<Season?> GetByIdWithWeeksAsync(int seasonId);

    /// <summary>
    /// Gets a season by ID with weeks and games loaded.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<Season?> GetByIdWithWeeksAndGamesAsync(int seasonId);

    /// <summary>
    /// Gets a season by ID with full data (weeks, games, teams).
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<Season?> GetByIdWithFullDataAsync(int seasonId);

    /// <summary>
    /// Gets all seasons for a league.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<List<Season>> GetByLeagueIdAsync(int leagueId);

    /// <summary>
    /// Gets the current/active season for a league.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<Season?> GetCurrentSeasonForLeagueAsync(int leagueId);

    /// <summary>
    /// Adds a new season.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<Season> AddAsync(Season season);

    /// <summary>
    /// Updates an existing season.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task UpdateAsync(Season season);

    /// <summary>
    /// Deletes a season.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task DeleteAsync(int seasonId);

    /// <summary>
    /// Soft deletes a season by marking it as deleted.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task SoftDeleteAsync(int seasonId, string? deletedBy = null, string? reason = null);

    /// <summary>
    /// Restores a soft-deleted season.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task RestoreAsync(int seasonId);

    /// <summary>
    /// Gets all soft-deleted seasons.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<List<Season>> GetDeletedAsync();

    /// <summary>
    /// Saves all pending changes.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<int> SaveChangesAsync();
}

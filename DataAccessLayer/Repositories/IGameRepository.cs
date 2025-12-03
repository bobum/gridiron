using DomainObjects;

namespace DataAccessLayer.Repositories;

/// <summary>
/// Repository interface for Game data access
/// This is the ONLY way to access game data from the database.
/// </summary>
public interface IGameRepository
{
    /// <summary>
    /// Gets all games.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<List<Game>> GetAllAsync();

    /// <summary>
    /// Gets a game by ID.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<Game?> GetByIdAsync(int gameId);

    /// <summary>
    /// Gets a game by ID with teams loaded.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<Game?> GetByIdWithTeamsAsync(int gameId);

    /// <summary>
    /// Gets a game by ID with teams and play-by-play data loaded.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<Game?> GetByIdWithPlayByPlayAsync(int gameId);

    /// <summary>
    /// Adds a new game.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<Game> AddAsync(Game game);

    /// <summary>
    /// Updates an existing game.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task UpdateAsync(Game game);

    /// <summary>
    /// Deletes a game.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task DeleteAsync(int gameId);

    /// <summary>
    /// Soft deletes a game by marking it as deleted.
    /// </summary>
    /// <param name="gameId">ID of the game to soft delete.</param>
    /// <param name="deletedBy">Username or identifier of who is deleting.</param>
    /// <param name="reason">Optional reason for deletion.</param>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task SoftDeleteAsync(int gameId, string? deletedBy = null, string? reason = null);

    /// <summary>
    /// Restores a soft-deleted game.
    /// </summary>
    /// <param name="gameId">ID of the game to restore.</param>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task RestoreAsync(int gameId);

    /// <summary>
    /// Gets all soft-deleted games.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<List<Game>> GetDeletedAsync();

    /// <summary>
    /// Saves all pending changes.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<int> SaveChangesAsync();
}

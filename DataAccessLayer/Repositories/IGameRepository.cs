using DomainObjects;

namespace DataAccessLayer.Repositories;

/// <summary>
/// Repository interface for Game data access
/// This is the ONLY way to access game data from the database
/// </summary>
public interface IGameRepository
{
    /// <summary>
    /// Gets all games
    /// </summary>
    Task<List<Game>> GetAllAsync();

    /// <summary>
    /// Gets a game by ID
    /// </summary>
    Task<Game?> GetByIdAsync(int gameId);

    /// <summary>
    /// Gets a game by ID with teams loaded
    /// </summary>
    Task<Game?> GetByIdWithTeamsAsync(int gameId);

    /// <summary>
    /// Adds a new game
    /// </summary>
    Task<Game> AddAsync(Game game);

    /// <summary>
    /// Updates an existing game
    /// </summary>
    Task UpdateAsync(Game game);

    /// <summary>
    /// Deletes a game
    /// </summary>
    Task DeleteAsync(int gameId);

    /// <summary>
    /// Saves all pending changes
    /// </summary>
    Task<int> SaveChangesAsync();
}

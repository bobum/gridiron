using DomainObjects;

namespace DataAccessLayer.Repositories;

/// <summary>
/// Repository interface for Player data access
/// This is the ONLY way to access player data from the database.
/// </summary>
public interface IPlayerRepository
{
    /// <summary>
    /// Gets all players.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<List<Player>> GetAllAsync();

    /// <summary>
    /// Gets all players for a specific team.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<List<Player>> GetByTeamIdAsync(int teamId);

    /// <summary>
    /// Gets a player by ID.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<Player?> GetByIdAsync(int playerId);

    /// <summary>
    /// Adds a new player.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<Player> AddAsync(Player player);

    /// <summary>
    /// Updates an existing player.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task UpdateAsync(Player player);

    /// <summary>
    /// Deletes a player.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task DeleteAsync(int playerId);

    /// <summary>
    /// Saves all pending changes.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<int> SaveChangesAsync();
}

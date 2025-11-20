using DomainObjects;

namespace DataAccessLayer.Repositories;

/// <summary>
/// Repository interface for Player data access
/// This is the ONLY way to access player data from the database
/// </summary>
public interface IPlayerRepository
{
    /// <summary>
    /// Gets all players
    /// </summary>
    Task<List<Player>> GetAllAsync();

    /// <summary>
    /// Gets all players for a specific team
    /// </summary>
    Task<List<Player>> GetByTeamIdAsync(int teamId);

    /// <summary>
    /// Gets a player by ID
    /// </summary>
    Task<Player?> GetByIdAsync(int playerId);

    /// <summary>
    /// Adds a new player
    /// </summary>
    Task<Player> AddAsync(Player player);

    /// <summary>
    /// Updates an existing player
    /// </summary>
    Task UpdateAsync(Player player);

    /// <summary>
    /// Deletes a player
    /// </summary>
    Task DeleteAsync(int playerId);

    /// <summary>
    /// Saves all pending changes
    /// </summary>
    Task<int> SaveChangesAsync();
}

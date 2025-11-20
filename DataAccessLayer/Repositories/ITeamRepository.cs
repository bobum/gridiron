using DomainObjects;

namespace DataAccessLayer.Repositories;

/// <summary>
/// Repository interface for Team data access
/// This is the ONLY way to access team data from the database
/// </summary>
public interface ITeamRepository
{
    /// <summary>
    /// Gets all teams
    /// </summary>
    Task<List<Team>> GetAllAsync();

    /// <summary>
    /// Gets a team by ID
    /// </summary>
    Task<Team?> GetByIdAsync(int teamId);

    /// <summary>
    /// Gets a team by ID with all players loaded
    /// </summary>
    Task<Team?> GetByIdWithPlayersAsync(int teamId);

    /// <summary>
    /// Gets a team by city and name
    /// </summary>
    Task<Team?> GetByCityAndNameAsync(string city, string name);

    /// <summary>
    /// Adds a new team
    /// </summary>
    Task<Team> AddAsync(Team team);

    /// <summary>
    /// Updates an existing team
    /// </summary>
    Task UpdateAsync(Team team);

    /// <summary>
    /// Deletes a team
    /// </summary>
    Task DeleteAsync(int teamId);

    /// <summary>
    /// Saves all pending changes
    /// </summary>
    Task<int> SaveChangesAsync();
}

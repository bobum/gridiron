using DomainObjects;

namespace GameManagement.Services;

/// <summary>
/// Service for building and managing teams
/// </summary>
public interface ITeamBuilderService
{
    /// <summary>
    /// Creates a new team with specified properties
    /// </summary>
    /// <param name="city">City name</param>
    /// <param name="name">Team name</param>
    /// <param name="budget">Team salary cap budget</param>
    /// <returns>A new team (not persisted to database)</returns>
    Team CreateTeam(string city, string name, decimal budget);

    /// <summary>
    /// Adds a player to a team's roster
    /// </summary>
    /// <param name="team">The team to add the player to</param>
    /// <param name="player">The player to add</param>
    /// <returns>True if added successfully, false if roster is full</returns>
    bool AddPlayerToTeam(Team team, Player player);

    /// <summary>
    /// Builds depth charts for all units (offense, defense, special teams)
    /// </summary>
    /// <param name="team">The team to build depth charts for</param>
    void AssignDepthCharts(Team team);

    /// <summary>
    /// Validates that a roster meets NFL requirements
    /// </summary>
    /// <param name="team">The team to validate</param>
    /// <returns>True if roster is valid, false otherwise</returns>
    bool ValidateRoster(Team team);
}

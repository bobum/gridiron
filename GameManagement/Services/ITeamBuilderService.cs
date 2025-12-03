using DomainObjects;

namespace GameManagement.Services;

/// <summary>
/// Service for building and managing teams.
/// </summary>
public interface ITeamBuilderService
{
    /// <summary>
    /// Creates a new team with specified properties.
    /// </summary>
    /// <param name="city">City name.</param>
    /// <param name="name">Team name.</param>
    /// <param name="budget">Team salary cap budget.</param>
    /// <returns>A new team (not persisted to database).</returns>
    Team CreateTeam(string city, string name, decimal budget);

    /// <summary>
    /// Adds a player to a team's roster.
    /// </summary>
    /// <param name="team">The team to add the player to.</param>
    /// <param name="player">The player to add.</param>
    /// <returns>True if added successfully, false if roster is full.</returns>
    bool AddPlayerToTeam(Team team, Player player);

    /// <summary>
    /// Builds depth charts for all units (offense, defense, special teams).
    /// </summary>
    /// <param name="team">The team to build depth charts for.</param>
    void AssignDepthCharts(Team team);

    /// <summary>
    /// Validates that a roster meets NFL requirements.
    /// </summary>
    /// <param name="team">The team to validate.</param>
    /// <returns>True if roster is valid, false otherwise.</returns>
    bool ValidateRoster(Team team);

    /// <summary>
    /// Populates a team with a full 53-player NFL roster.
    /// </summary>
    /// <param name="team">The team to populate.</param>
    /// <param name="seed">Optional seed for reproducible generation.</param>
    /// <returns>The team with populated roster.</returns>
    Team PopulateTeamRoster(Team team, int? seed = null);

    /// <summary>
    /// Updates a team with new values.
    /// </summary>
    /// <param name="team">The team to update.</param>
    /// <param name="newName">Optional new name for the team.</param>
    /// <param name="newCity">Optional new city for the team.</param>
    /// <param name="newBudget">Optional new budget for the team.</param>
    /// <param name="newChampionships">Optional new championships count.</param>
    /// <param name="newWins">Optional new wins count.</param>
    /// <param name="newLosses">Optional new losses count.</param>
    /// <param name="newTies">Optional new ties count.</param>
    /// <param name="newFanSupport">Optional new fan support (0-100).</param>
    /// <param name="newChemistry">Optional new chemistry (0-100).</param>
    void UpdateTeam(Team team, string? newName, string? newCity, int? newBudget,
        int? newChampionships, int? newWins, int? newLosses, int? newTies,
        int? newFanSupport, int? newChemistry);
}

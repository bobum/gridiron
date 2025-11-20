using DomainObjects;

namespace GameManagement.Services;

/// <summary>
/// Service for handling player progression, aging, and retirement
/// </summary>
public interface IPlayerProgressionService
{
    /// <summary>
    /// Ages a player by one year and applies age curve adjustments
    /// </summary>
    /// <param name="player">The player to age</param>
    /// <returns>True if player is still active, false if retired</returns>
    bool AgePlayerOneYear(Player player);

    /// <summary>
    /// Calculates a player's overall rating based on position-relevant attributes
    /// </summary>
    /// <param name="player">The player to evaluate</param>
    /// <returns>Overall rating (0-100)</returns>
    int CalculateOverallRating(Player player);

    /// <summary>
    /// Determines if a player should retire based on age and performance
    /// </summary>
    /// <param name="player">The player to evaluate</param>
    /// <returns>True if player should retire</returns>
    bool ShouldRetire(Player player);
}

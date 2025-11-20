using DomainObjects;

namespace GameManagement.Services;

/// <summary>
/// Service for generating random players and draft classes
/// </summary>
public interface IPlayerGeneratorService
{
    /// <summary>
    /// Generates a random player for a specific position
    /// </summary>
    /// <param name="position">The position to generate</param>
    /// <param name="seed">Optional seed for reproducible generation</param>
    /// <returns>A randomly generated player</returns>
    Player GenerateRandomPlayer(Positions position, int? seed = null);

    /// <summary>
    /// Generates a full draft class with rookie players
    /// </summary>
    /// <param name="year">The draft year</param>
    /// <param name="rounds">Number of rounds (default 7)</param>
    /// <returns>List of draft-eligible players</returns>
    List<Player> GenerateDraftClass(int year, int rounds = 7);

    /// <summary>
    /// Generates multiple players for testing purposes
    /// </summary>
    /// <param name="count">Number of players to generate</param>
    /// <param name="seed">Optional seed for reproducible generation</param>
    /// <returns>List of randomly generated players across all positions</returns>
    List<Player> GenerateMultiplePlayers(int count, int? seed = null);
}

using DomainObjects;
using Gridiron.WebApi.DTOs;

namespace Gridiron.WebApi.Services;

/// <summary>
/// Service for running game simulations.
/// </summary>
public interface IGameSimulationService
{
    /// <summary>
    /// Simulates a game asynchronously.
    /// </summary>
    /// <param name="homeTeamId">ID of the home team.</param>
    /// <param name="awayTeamId">ID of the away team.</param>
    /// <param name="randomSeed">Optional seed for reproducible simulation.</param>
    /// <returns>The completed game with all plays and results.</returns>
    Task<Game> SimulateGameAsync(int homeTeamId, int awayTeamId, int? randomSeed = null);

    /// <summary>
    /// Gets a game by ID.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<Game?> GetGameAsync(int gameId);

    /// <summary>
    /// Gets all games.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<List<Game>> GetGamesAsync();

    /// <summary>
    /// Gets plays for a game by deserializing from PlayByPlay.PlaysJson.
    /// </summary>
    /// <param name="gameId">ID of the game.</param>
    /// <returns>List of plays as DTOs, or null if game not found.</returns>
    Task<List<PlayDto>?> GetGamePlaysAsync(int gameId);
}

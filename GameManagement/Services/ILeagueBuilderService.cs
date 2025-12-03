using DomainObjects;

namespace GameManagement.Services;

/// <summary>
/// Service for building and managing leagues.
/// </summary>
public interface ILeagueBuilderService
{
    /// <summary>
    /// Creates a new league with specified structure (conferences, divisions, teams).
    /// </summary>
    /// <param name="leagueName">Name of the league.</param>
    /// <param name="numberOfConferences">Number of conferences in the league.</param>
    /// <param name="divisionsPerConference">Number of divisions per conference.</param>
    /// <param name="teamsPerDivision">Number of teams per division.</param>
    /// <returns>A new league with complete structure (not persisted to database).</returns>
    League CreateLeague(string leagueName, int numberOfConferences, int divisionsPerConference, int teamsPerDivision);

    /// <summary>
    /// Populates all teams in a league with full 53-player rosters.
    /// </summary>
    /// <param name="league">The league to populate.</param>
    /// <param name="seed">Optional seed for reproducible generation.</param>
    /// <returns>The league with all teams populated.</returns>
    League PopulateLeagueRosters(League league, int? seed = null);

    /// <summary>
    /// Updates a league with new values.
    /// </summary>
    /// <param name="league">The league to update.</param>
    /// <param name="newName">Optional new name for the league.</param>
    /// <param name="newSeason">Optional new season for the league.</param>
    /// <param name="newIsActive">Optional new active status for the league.</param>
    void UpdateLeague(League league, string? newName, int? newSeason, bool? newIsActive);
}

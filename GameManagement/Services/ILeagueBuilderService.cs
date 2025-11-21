using DomainObjects;

namespace GameManagement.Services;

/// <summary>
/// Service for building and managing leagues
/// </summary>
public interface ILeagueBuilderService
{
    /// <summary>
    /// Creates a new league with specified structure (conferences, divisions, teams)
    /// </summary>
    /// <param name="leagueName">Name of the league</param>
    /// <param name="numberOfConferences">Number of conferences in the league</param>
    /// <param name="divisionsPerConference">Number of divisions per conference</param>
    /// <param name="teamsPerDivision">Number of teams per division</param>
    /// <returns>A new league with complete structure (not persisted to database)</returns>
    League CreateLeague(string leagueName, int numberOfConferences, int divisionsPerConference, int teamsPerDivision);
}

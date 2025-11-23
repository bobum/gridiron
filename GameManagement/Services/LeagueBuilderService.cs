using DomainObjects;
using Microsoft.Extensions.Logging;

namespace GameManagement.Services;

/// <summary>
/// Service for building and managing leagues
/// </summary>
public class LeagueBuilderService : ILeagueBuilderService
{
    private readonly ILogger<LeagueBuilderService> _logger;
    private readonly ITeamBuilderService _teamBuilder;

    public LeagueBuilderService(
        ILogger<LeagueBuilderService> logger,
        ITeamBuilderService teamBuilder)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _teamBuilder = teamBuilder ?? throw new ArgumentNullException(nameof(teamBuilder));
    }

    public League CreateLeague(string leagueName, int numberOfConferences, int divisionsPerConference, int teamsPerDivision)
    {
        // Validate inputs
        if (string.IsNullOrWhiteSpace(leagueName))
            throw new ArgumentException("League name cannot be empty", nameof(leagueName));

        if (numberOfConferences <= 0)
            throw new ArgumentException("Number of conferences must be greater than 0", nameof(numberOfConferences));

        if (divisionsPerConference <= 0)
            throw new ArgumentException("Divisions per conference must be greater than 0", nameof(divisionsPerConference));

        if (teamsPerDivision <= 0)
            throw new ArgumentException("Teams per division must be greater than 0", nameof(teamsPerDivision));

        _logger.LogInformation(
            "Creating league '{LeagueName}' with {Conferences} conferences, {Divisions} divisions per conference, {Teams} teams per division",
            leagueName, numberOfConferences, divisionsPerConference, teamsPerDivision);

        var league = new League
        {
            Name = leagueName,
            Season = DateTime.Now.Year,
            IsActive = true,
            Conferences = new List<Conference>()
        };

        // Create conferences
        for (int confNum = 1; confNum <= numberOfConferences; confNum++)
        {
            var conference = new Conference
            {
                Name = $"Conference {confNum}",
                Divisions = new List<Division>()
            };

            // Create divisions for this conference
            for (int divNum = 1; divNum <= divisionsPerConference; divNum++)
            {
                var division = new Division
                {
                    Name = $"Division {divNum}",
                    Teams = new List<Team>()
                };

                // Create teams for this division
                for (int teamNum = 1; teamNum <= teamsPerDivision; teamNum++)
                {
                    var city = $"City {confNum}-{divNum}-{teamNum}";
                    var name = $"Team {teamNum}";
                    var team = _teamBuilder.CreateTeam(city, name, 0);
                    division.Teams.Add(team);
                }

                conference.Divisions.Add(division);
            }

            league.Conferences.Add(conference);
        }

        int totalTeams = numberOfConferences * divisionsPerConference * teamsPerDivision;
        _logger.LogInformation(
            "Successfully created league '{LeagueName}' with {TotalTeams} teams",
            leagueName, totalTeams);

        return league;
    }

    public League PopulateLeagueRosters(League league, int? seed = null)
    {
        if (league == null)
            throw new ArgumentNullException(nameof(league));

        _logger.LogInformation(
            "Populating rosters for all teams in league '{LeagueName}' (seed: {Seed})",
            league.Name, seed?.ToString() ?? "random");

        int teamCount = 0;
        int teamIndex = 0;

        // Iterate through all teams in the league hierarchy
        foreach (var conference in league.Conferences)
        {
            foreach (var division in conference.Divisions)
            {
                foreach (var team in division.Teams)
                {
                    // Use seed + team index for varied but reproducible generation
                    var teamSeed = seed.HasValue ? seed.Value + (teamIndex * 1000) : (int?)null;

                    _teamBuilder.PopulateTeamRoster(team, teamSeed);
                    teamCount++;
                    teamIndex++;
                }
            }
        }

        _logger.LogInformation(
            "Successfully populated rosters for {TeamCount} teams in league '{LeagueName}'",
            teamCount, league.Name);

        return league;
    }

    public void UpdateLeague(League league, string? newName, int? newSeason, bool? newIsActive)
    {
        if (league == null)
            throw new ArgumentNullException(nameof(league));

        // Update name if provided and not empty
        if (!string.IsNullOrWhiteSpace(newName))
        {
            _logger.LogInformation(
                "Updating league {LeagueId} name from '{OldName}' to '{NewName}'",
                league.Id, league.Name, newName);

            league.Name = newName;
        }

        // Update season if provided and valid
        if (newSeason.HasValue)
        {
            // Validate season is reasonable (1900 to current year + 5)
            int currentYear = DateTime.Now.Year;
            if (newSeason.Value < 1900 || newSeason.Value > currentYear + 5)
            {
                throw new ArgumentException(
                    $"Season must be between 1900 and {currentYear + 5}",
                    nameof(newSeason));
            }

            _logger.LogInformation(
                "Updating league {LeagueId} season from {OldSeason} to {NewSeason}",
                league.Id, league.Season, newSeason.Value);

            league.Season = newSeason.Value;
        }

        // Update IsActive if provided
        if (newIsActive.HasValue)
        {
            _logger.LogInformation(
                "Updating league {LeagueId} active status from {OldStatus} to {NewStatus}",
                league.Id, league.IsActive, newIsActive.Value);

            league.IsActive = newIsActive.Value;
        }
    }
}

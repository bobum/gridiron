using DomainObjects;
using Microsoft.Extensions.Logging;

namespace GameManagement.Services;

/// <summary>
/// Service for building and managing leagues
/// </summary>
public class LeagueBuilderService : ILeagueBuilderService
{
    private readonly ILogger<LeagueBuilderService> _logger;

    public LeagueBuilderService(ILogger<LeagueBuilderService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
                    var team = CreateTeamWithDefaults(confNum, divNum, teamNum);
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

    private Team CreateTeamWithDefaults(int conferenceNum, int divisionNum, int teamNum)
    {
        return new Team
        {
            Name = $"Team {teamNum}",
            City = $"City {conferenceNum}-{divisionNum}-{teamNum}",
            Players = new List<Player>(),  // Empty roster
            Budget = 0,
            Championships = 0,
            Wins = 0,
            Losses = 0,
            Ties = 0,
            FanSupport = 50,
            Chemistry = 50,

            // Initialize empty depth charts
            OffenseDepthChart = new DepthChart(),
            DefenseDepthChart = new DepthChart(),
            FieldGoalOffenseDepthChart = new DepthChart(),
            FieldGoalDefenseDepthChart = new DepthChart(),
            KickoffOffenseDepthChart = new DepthChart(),
            KickoffDefenseDepthChart = new DepthChart(),
            PuntOffenseDepthChart = new DepthChart(),
            PuntDefenseDepthChart = new DepthChart(),

            // Initialize empty staff
            HeadCoach = new Coach(),
            OffensiveCoordinator = new Coach(),
            DefensiveCoordinator = new Coach(),
            SpecialTeamsCoordinator = new Coach(),
            AssistantCoaches = new List<Coach>(),

            HeadAthleticTrainer = new Trainer(),
            TeamDoctor = new Trainer(),

            DirectorOfScouting = new Scout(),
            CollegeScouts = new List<Scout>(),
            ProScouts = new List<Scout>()
        };
    }
}

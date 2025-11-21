using DomainObjects;
using FluentAssertions;
using GameManagement.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GameManagement.Tests;

/// <summary>
/// Comprehensive tests for LeagueBuilderService
/// </summary>
public class LeagueBuilderServiceTests
{
    private readonly Mock<ILogger<LeagueBuilderService>> _loggerMock;
    private readonly Mock<ITeamBuilderService> _teamBuilderMock;
    private readonly LeagueBuilderService _service;

    public LeagueBuilderServiceTests()
    {
        _loggerMock = new Mock<ILogger<LeagueBuilderService>>();
        _teamBuilderMock = new Mock<ITeamBuilderService>();
        _service = new LeagueBuilderService(_loggerMock.Object, _teamBuilderMock.Object);
    }

    #region CreateLeague Tests - Valid Parameters

    [Fact]
    public void CreateLeague_WithValidParameters_ShouldCreateLeagueSuccessfully()
    {
        // Arrange
        var leagueName = "National Football League";
        var numConferences = 2;
        var divisionsPerConference = 4;
        var teamsPerDivision = 4;

        // Act
        var league = _service.CreateLeague(leagueName, numConferences, divisionsPerConference, teamsPerDivision);

        // Assert
        league.Should().NotBeNull();
        league.Name.Should().Be(leagueName);
        league.Season.Should().Be(DateTime.Now.Year);
        league.IsActive.Should().BeTrue();
        league.Conferences.Should().NotBeNull();
    }

    [Fact]
    public void CreateLeague_ShouldCreateCorrectNumberOfConferences()
    {
        // Arrange
        var leagueName = "NFL";
        var numConferences = 2;
        var divisionsPerConference = 4;
        var teamsPerDivision = 4;

        // Act
        var league = _service.CreateLeague(leagueName, numConferences, divisionsPerConference, teamsPerDivision);

        // Assert
        league.Conferences.Should().HaveCount(numConferences);
        league.Conferences[0].Name.Should().Be("Conference 1");
        league.Conferences[1].Name.Should().Be("Conference 2");
    }

    [Fact]
    public void CreateLeague_ShouldCreateCorrectNumberOfDivisionsPerConference()
    {
        // Arrange
        var numConferences = 2;
        var divisionsPerConference = 4;
        var teamsPerDivision = 4;

        // Act
        var league = _service.CreateLeague("NFL", numConferences, divisionsPerConference, teamsPerDivision);

        // Assert
        foreach (var conference in league.Conferences)
        {
            conference.Divisions.Should().HaveCount(divisionsPerConference);
        }

        // Verify division names
        league.Conferences[0].Divisions[0].Name.Should().Be("Division 1");
        league.Conferences[0].Divisions[1].Name.Should().Be("Division 2");
        league.Conferences[0].Divisions[2].Name.Should().Be("Division 3");
        league.Conferences[0].Divisions[3].Name.Should().Be("Division 4");
    }

    [Fact]
    public void CreateLeague_ShouldCreateCorrectNumberOfTeamsPerDivision()
    {
        // Arrange
        var numConferences = 2;
        var divisionsPerConference = 4;
        var teamsPerDivision = 4;

        // Act
        var league = _service.CreateLeague("NFL", numConferences, divisionsPerConference, teamsPerDivision);

        // Assert
        foreach (var conference in league.Conferences)
        {
            foreach (var division in conference.Divisions)
            {
                division.Teams.Should().HaveCount(teamsPerDivision);
            }
        }
    }

    [Fact]
    public void CreateLeague_ShouldCreateTeamsWithPlaceholderNames()
    {
        // Arrange & Act
        var league = _service.CreateLeague("NFL", 1, 1, 3);

        // Assert
        var division = league.Conferences[0].Divisions[0];
        division.Teams[0].Name.Should().Be("Team 1");
        division.Teams[1].Name.Should().Be("Team 2");
        division.Teams[2].Name.Should().Be("Team 3");
    }

    [Fact]
    public void CreateLeague_ShouldCreateTeamsWithEmptyRosters()
    {
        // Arrange & Act
        var league = _service.CreateLeague("NFL", 2, 2, 2);

        // Assert
        foreach (var conference in league.Conferences)
        {
            foreach (var division in conference.Divisions)
            {
                foreach (var team in division.Teams)
                {
                    team.Players.Should().NotBeNull().And.BeEmpty();
                }
            }
        }
    }

    [Fact]
    public void CreateLeague_ShouldInitializeTeamsWithDefaultValues()
    {
        // Arrange & Act
        var league = _service.CreateLeague("NFL", 1, 1, 1);

        // Assert
        var team = league.Conferences[0].Divisions[0].Teams[0];
        team.Budget.Should().Be(0);
        team.Championships.Should().Be(0);
        team.Wins.Should().Be(0);
        team.Losses.Should().Be(0);
        team.Ties.Should().Be(0);
        team.FanSupport.Should().Be(50);
        team.Chemistry.Should().Be(50);
    }

    [Fact]
    public void CreateLeague_ShouldInitializeTeamsWithDepthCharts()
    {
        // Arrange & Act
        var league = _service.CreateLeague("NFL", 1, 1, 1);

        // Assert
        var team = league.Conferences[0].Divisions[0].Teams[0];
        team.OffenseDepthChart.Should().NotBeNull();
        team.DefenseDepthChart.Should().NotBeNull();
        team.FieldGoalOffenseDepthChart.Should().NotBeNull();
        team.FieldGoalDefenseDepthChart.Should().NotBeNull();
        team.KickoffOffenseDepthChart.Should().NotBeNull();
        team.KickoffDefenseDepthChart.Should().NotBeNull();
        team.PuntOffenseDepthChart.Should().NotBeNull();
        team.PuntDefenseDepthChart.Should().NotBeNull();
    }

    [Fact]
    public void CreateLeague_ShouldInitializeTeamsWithStaff()
    {
        // Arrange & Act
        var league = _service.CreateLeague("NFL", 1, 1, 1);

        // Assert
        var team = league.Conferences[0].Divisions[0].Teams[0];
        team.HeadCoach.Should().NotBeNull();
        team.OffensiveCoordinator.Should().NotBeNull();
        team.DefensiveCoordinator.Should().NotBeNull();
        team.SpecialTeamsCoordinator.Should().NotBeNull();
        team.AssistantCoaches.Should().NotBeNull();
        team.HeadAthleticTrainer.Should().NotBeNull();
        team.TeamDoctor.Should().NotBeNull();
        team.DirectorOfScouting.Should().NotBeNull();
        team.CollegeScouts.Should().NotBeNull();
        team.ProScouts.Should().NotBeNull();
    }

    [Theory]
    [InlineData(2, 4, 4, 32)]  // NFL structure
    [InlineData(1, 1, 1, 1)]   // Minimal league
    [InlineData(4, 2, 3, 24)]  // Custom structure
    public void CreateLeague_ShouldCreateCorrectTotalNumberOfTeams(
        int conferences, int divisions, int teams, int expectedTotal)
    {
        // Arrange & Act
        var league = _service.CreateLeague("Test League", conferences, divisions, teams);

        // Assert
        var actualTotal = league.Conferences
            .SelectMany(c => c.Divisions)
            .SelectMany(d => d.Teams)
            .Count();

        actualTotal.Should().Be(expectedTotal);
    }

    #endregion

    #region CreateLeague Tests - Invalid Parameters

    [Theory]
    [InlineData("", 2, 4, 4)]
    [InlineData(null, 2, 4, 4)]
    [InlineData("   ", 2, 4, 4)]
    public void CreateLeague_WithEmptyLeagueName_ShouldThrowArgumentException(
        string leagueName, int conferences, int divisions, int teams)
    {
        // Act & Assert
        var act = () => _service.CreateLeague(leagueName, conferences, divisions, teams);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*League name*")
            .And.ParamName.Should().Be("leagueName");
    }

    [Theory]
    [InlineData(0, 4, 4)]
    [InlineData(-1, 4, 4)]
    [InlineData(-10, 4, 4)]
    public void CreateLeague_WithInvalidNumberOfConferences_ShouldThrowArgumentException(
        int conferences, int divisions, int teams)
    {
        // Act & Assert
        var act = () => _service.CreateLeague("NFL", conferences, divisions, teams);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Number of conferences*")
            .And.ParamName.Should().Be("numberOfConferences");
    }

    [Theory]
    [InlineData(2, 0, 4)]
    [InlineData(2, -1, 4)]
    [InlineData(2, -10, 4)]
    public void CreateLeague_WithInvalidDivisionsPerConference_ShouldThrowArgumentException(
        int conferences, int divisions, int teams)
    {
        // Act & Assert
        var act = () => _service.CreateLeague("NFL", conferences, divisions, teams);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Divisions per conference*")
            .And.ParamName.Should().Be("divisionsPerConference");
    }

    [Theory]
    [InlineData(2, 4, 0)]
    [InlineData(2, 4, -1)]
    [InlineData(2, 4, -10)]
    public void CreateLeague_WithInvalidTeamsPerDivision_ShouldThrowArgumentException(
        int conferences, int divisions, int teams)
    {
        // Act & Assert
        var act = () => _service.CreateLeague("NFL", conferences, divisions, teams);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Teams per division*")
            .And.ParamName.Should().Be("teamsPerDivision");
    }

    #endregion

    #region CreateLeague Tests - Hierarchical Structure

    [Fact]
    public void CreateLeague_ShouldCreateCompleteHierarchy()
    {
        // Arrange & Act
        var league = _service.CreateLeague("NFL", 2, 2, 2);

        // Assert - Verify complete hierarchy
        league.Should().NotBeNull();
        league.Conferences.Should().HaveCount(2);

        // Conference 1
        league.Conferences[0].Should().NotBeNull();
        league.Conferences[0].Divisions.Should().HaveCount(2);
        league.Conferences[0].Divisions[0].Teams.Should().HaveCount(2);
        league.Conferences[0].Divisions[1].Teams.Should().HaveCount(2);

        // Conference 2
        league.Conferences[1].Should().NotBeNull();
        league.Conferences[1].Divisions.Should().HaveCount(2);
        league.Conferences[1].Divisions[0].Teams.Should().HaveCount(2);
        league.Conferences[1].Divisions[1].Teams.Should().HaveCount(2);
    }

    [Fact]
    public void CreateLeague_ShouldAssignUniqueNamesToAllEntities()
    {
        // Arrange & Act
        var league = _service.CreateLeague("Test League", 2, 3, 4);

        // Assert - Conference names should be unique
        var conferenceNames = league.Conferences.Select(c => c.Name).ToList();
        conferenceNames.Should().OnlyHaveUniqueItems();
        conferenceNames.Should().Contain("Conference 1");
        conferenceNames.Should().Contain("Conference 2");

        // Division names should be numbered but can repeat across conferences
        foreach (var conference in league.Conferences)
        {
            var divisionNames = conference.Divisions.Select(d => d.Name).ToList();
            divisionNames.Should().OnlyHaveUniqueItems();
            divisionNames.Should().Contain("Division 1");
            divisionNames.Should().Contain("Division 2");
            divisionNames.Should().Contain("Division 3");
        }

        // Team names should be numbered but can repeat across divisions
        foreach (var conference in league.Conferences)
        {
            foreach (var division in conference.Divisions)
            {
                var teamNames = division.Teams.Select(t => t.Name).ToList();
                teamNames.Should().OnlyHaveUniqueItems();
                teamNames.Should().Contain("Team 1");
                teamNames.Should().Contain("Team 2");
                teamNames.Should().Contain("Team 3");
                teamNames.Should().Contain("Team 4");
            }
        }
    }

    #endregion

    #region PopulateLeagueRosters Tests

    [Fact]
    public void PopulateLeagueRosters_WithValidLeague_ShouldPopulateAllTeams()
    {
        // Arrange
        var league = _service.CreateLeague("NFL", 2, 2, 2); // 8 teams total
        var populateCallCount = 0;

        _teamBuilderMock
            .Setup(t => t.PopulateTeamRoster(It.IsAny<Team>(), It.IsAny<int?>()))
            .Returns((Team team, int? seed) =>
            {
                populateCallCount++;
                return team;
            });

        // Act
        var result = _service.PopulateLeagueRosters(league);

        // Assert
        result.Should().NotBeNull();
        populateCallCount.Should().Be(8); // Should call PopulateTeamRoster 8 times
        _teamBuilderMock.Verify(t => t.PopulateTeamRoster(It.IsAny<Team>(), It.IsAny<int?>()), Times.Exactly(8));
    }

    [Fact]
    public void PopulateLeagueRosters_WithNullLeague_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => _service.PopulateLeagueRosters(null!);
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("league");
    }

    [Fact]
    public void PopulateLeagueRosters_ShouldReturnSameLeagueInstance()
    {
        // Arrange
        var league = _service.CreateLeague("Test League", 1, 1, 1);

        _teamBuilderMock
            .Setup(t => t.PopulateTeamRoster(It.IsAny<Team>(), It.IsAny<int?>()))
            .Returns((Team team, int? seed) => team);

        // Act
        var result = _service.PopulateLeagueRosters(league);

        // Assert
        result.Should().BeSameAs(league);
    }

    [Fact]
    public void PopulateLeagueRosters_WithSeed_ShouldPassIncrementingSeedsToTeams()
    {
        // Arrange
        var league = _service.CreateLeague("NFL", 1, 1, 3); // 3 teams
        var baseSeed = 5000;
        var seedsReceived = new List<int?>();

        _teamBuilderMock
            .Setup(t => t.PopulateTeamRoster(It.IsAny<Team>(), It.IsAny<int?>()))
            .Returns((Team team, int? seed) =>
            {
                seedsReceived.Add(seed);
                return team;
            });

        // Act
        _service.PopulateLeagueRosters(league, baseSeed);

        // Assert
        seedsReceived.Should().HaveCount(3);
        seedsReceived[0].Should().Be(baseSeed);         // Team 0 gets seed
        seedsReceived[1].Should().Be(baseSeed + 1000);  // Team 1 gets seed + 1000
        seedsReceived[2].Should().Be(baseSeed + 2000);  // Team 2 gets seed + 2000
    }

    [Fact]
    public void PopulateLeagueRosters_WithNullSeed_ShouldPassNullSeedsToTeams()
    {
        // Arrange
        var league = _service.CreateLeague("NFL", 1, 1, 2);
        var seedsReceived = new List<int?>();

        _teamBuilderMock
            .Setup(t => t.PopulateTeamRoster(It.IsAny<Team>(), It.IsAny<int?>()))
            .Returns((Team team, int? seed) =>
            {
                seedsReceived.Add(seed);
                return team;
            });

        // Act
        _service.PopulateLeagueRosters(league, null);

        // Assert
        seedsReceived.Should().HaveCount(2);
        seedsReceived.Should().OnlyContain(s => s == null);
    }

    [Theory]
    [InlineData(2, 4, 4, 32)]  // NFL structure - 32 teams
    [InlineData(1, 1, 1, 1)]   // Minimal league - 1 team
    [InlineData(2, 2, 3, 12)]  // Custom structure - 12 teams
    public void PopulateLeagueRosters_ShouldPopulateCorrectNumberOfTeams(
        int conferences, int divisions, int teams, int expectedTeams)
    {
        // Arrange
        var league = _service.CreateLeague("Test League", conferences, divisions, teams);
        var populateCallCount = 0;

        _teamBuilderMock
            .Setup(t => t.PopulateTeamRoster(It.IsAny<Team>(), It.IsAny<int?>()))
            .Returns((Team team, int? seed) =>
            {
                populateCallCount++;
                return team;
            });

        // Act
        _service.PopulateLeagueRosters(league);

        // Assert
        populateCallCount.Should().Be(expectedTeams);
    }

    [Fact]
    public void PopulateLeagueRosters_ShouldPopulateTeamsInAllConferences()
    {
        // Arrange
        var league = _service.CreateLeague("NFL", 3, 2, 2); // 3 conferences, 2 divisions each, 2 teams each = 12 teams
        var populatedTeams = new List<Team>();

        _teamBuilderMock
            .Setup(t => t.PopulateTeamRoster(It.IsAny<Team>(), It.IsAny<int?>()))
            .Returns((Team team, int? seed) =>
            {
                populatedTeams.Add(team);
                return team;
            });

        // Act
        _service.PopulateLeagueRosters(league);

        // Assert
        populatedTeams.Should().HaveCount(12);

        // Verify teams from all conferences were populated
        var allTeamsInLeague = league.Conferences
            .SelectMany(c => c.Divisions)
            .SelectMany(d => d.Teams)
            .ToList();

        populatedTeams.Should().BeEquivalentTo(allTeamsInLeague);
    }

    [Fact]
    public void PopulateLeagueRosters_WithDifferentSeeds_ShouldProduceDifferentTeamSeeds()
    {
        // Arrange
        var league1 = _service.CreateLeague("League1", 1, 1, 2);
        var league2 = _service.CreateLeague("League2", 1, 1, 2);
        var seeds1 = new List<int?>();
        var seeds2 = new List<int?>();

        _teamBuilderMock
            .Setup(t => t.PopulateTeamRoster(It.IsAny<Team>(), It.IsAny<int?>()))
            .Returns((Team team, int? seed) =>
            {
                if (team.Name == "Team 1" || team.Name == "Team 2")
                {
                    if (seeds1.Count < 2)
                        seeds1.Add(seed);
                    else
                        seeds2.Add(seed);
                }
                return team;
            });

        // Act
        _service.PopulateLeagueRosters(league1, 1000);
        _service.PopulateLeagueRosters(league2, 2000);

        // Assert
        seeds1[0].Should().Be(1000);
        seeds1[1].Should().Be(2000);
        seeds2[0].Should().Be(2000);
        seeds2[1].Should().Be(3000);
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new LeagueBuilderService(null!, _teamBuilderMock.Object);
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("logger");
    }

    [Fact]
    public void Constructor_WithNullTeamBuilder_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new LeagueBuilderService(_loggerMock.Object, null!);
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("teamBuilder");
    }

    #endregion
}

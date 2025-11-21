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
    private readonly LeagueBuilderService _service;

    public LeagueBuilderServiceTests()
    {
        _loggerMock = new Mock<ILogger<LeagueBuilderService>>();
        _service = new LeagueBuilderService(_loggerMock.Object);
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

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new LeagueBuilderService(null!);
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("logger");
    }

    #endregion
}

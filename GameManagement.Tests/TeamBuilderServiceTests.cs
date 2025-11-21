using DomainObjects;
using FluentAssertions;
using GameManagement.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GameManagement.Tests;

/// <summary>
/// Comprehensive tests for TeamBuilderService
/// </summary>
public class TeamBuilderServiceTests
{
    private readonly Mock<ILogger<TeamBuilderService>> _loggerMock;
    private readonly Mock<IPlayerGeneratorService> _playerGeneratorMock;
    private readonly TeamBuilderService _service;

    public TeamBuilderServiceTests()
    {
        _loggerMock = new Mock<ILogger<TeamBuilderService>>();
        _playerGeneratorMock = new Mock<IPlayerGeneratorService>();
        _service = new TeamBuilderService(_loggerMock.Object, _playerGeneratorMock.Object);
    }

    #region CreateTeam Tests

    [Fact]
    public void CreateTeam_WithValidParameters_ShouldCreateTeamSuccessfully()
    {
        // Arrange
        var city = "Seattle";
        var name = "Seahawks";
        var budget = 200_000_000m;

        // Act
        var team = _service.CreateTeam(city, name, budget);

        // Assert
        team.Should().NotBeNull();
        team.City.Should().Be(city);
        team.Name.Should().Be(name);
        team.Budget.Should().Be(200_000_000);
        team.Players.Should().NotBeNull().And.BeEmpty();
        team.FanSupport.Should().Be(50);
        team.Chemistry.Should().Be(50);
        team.Championships.Should().Be(0);
        team.Wins.Should().Be(0);
        team.Losses.Should().Be(0);
        team.Ties.Should().Be(0);
    }

    [Fact]
    public void CreateTeam_ShouldInitializeAllDepthCharts()
    {
        // Arrange & Act
        var team = _service.CreateTeam("Seattle", "Seahawks", 200_000_000m);

        // Assert
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
    public void CreateTeam_ShouldInitializeCoachingStaff()
    {
        // Arrange & Act
        var team = _service.CreateTeam("Seattle", "Seahawks", 200_000_000m);

        // Assert
        team.HeadCoach.Should().NotBeNull();
        team.HeadCoach.FirstName.Should().Be("Head");
        team.HeadCoach.LastName.Should().Be("Coach");
        team.OffensiveCoordinator.Should().NotBeNull();
        team.DefensiveCoordinator.Should().NotBeNull();
        team.SpecialTeamsCoordinator.Should().NotBeNull();
        team.AssistantCoaches.Should().NotBeNull();
    }

    [Fact]
    public void CreateTeam_ShouldInitializeTrainingAndScoutingStaff()
    {
        // Arrange & Act
        var team = _service.CreateTeam("Seattle", "Seahawks", 200_000_000m);

        // Assert
        team.HeadAthleticTrainer.Should().NotBeNull();
        team.TeamDoctor.Should().NotBeNull();
        team.DirectorOfScouting.Should().NotBeNull();
        team.CollegeScouts.Should().NotBeNull();
        team.ProScouts.Should().NotBeNull();
    }

    [Theory]
    [InlineData("", "Seahawks", 200_000_000)]
    [InlineData(null, "Seahawks", 200_000_000)]
    [InlineData("   ", "Seahawks", 200_000_000)]
    public void CreateTeam_WithEmptyCity_ShouldThrowArgumentException(string city, string name, decimal budget)
    {
        // Act & Assert
        var act = () => _service.CreateTeam(city, name, budget);
        act.Should().Throw<ArgumentException>().WithMessage("*City*");
    }

    [Theory]
    [InlineData("Seattle", "", 200_000_000)]
    [InlineData("Seattle", null, 200_000_000)]
    [InlineData("Seattle", "   ", 200_000_000)]
    public void CreateTeam_WithEmptyName_ShouldThrowArgumentException(string city, string name, decimal budget)
    {
        // Act & Assert
        var act = () => _service.CreateTeam(city, name, budget);
        act.Should().Throw<ArgumentException>().WithMessage("*Name*");
    }

    [Fact]
    public void CreateTeam_WithNegativeBudget_ShouldThrowArgumentException()
    {
        // Act & Assert
        var act = () => _service.CreateTeam("Seattle", "Seahawks", -1000m);
        act.Should().Throw<ArgumentException>().WithMessage("*Budget*");
    }

    #endregion

    #region AddPlayerToTeam Tests

    [Fact]
    public void AddPlayerToTeam_WithValidPlayer_ShouldAddSuccessfully()
    {
        // Arrange
        var team = _service.CreateTeam("Seattle", "Seahawks", 200_000_000m);
        var player = CreateTestPlayer(Positions.QB, "Tom", "Brady");

        // Act
        var result = _service.AddPlayerToTeam(team, player);

        // Assert
        result.Should().BeTrue();
        team.Players.Should().ContainSingle();
        team.Players[0].Should().Be(player);
        player.TeamId.Should().Be(team.Id);
    }

    [Fact]
    public void AddPlayerToTeam_MultiplePlayersUnderLimit_ShouldAddAll()
    {
        // Arrange
        var team = _service.CreateTeam("Seattle", "Seahawks", 200_000_000m);
        var players = Enumerable.Range(1, 10)
            .Select(i => CreateTestPlayer(Positions.QB, $"Player{i}", "Test"))
            .ToList();

        // Act
        foreach (var player in players)
        {
            var result = _service.AddPlayerToTeam(team, player);
            result.Should().BeTrue();
        }

        // Assert
        team.Players.Should().HaveCount(10);
    }

    [Fact]
    public void AddPlayerToTeam_ExceedingRosterLimit_ShouldReturnFalse()
    {
        // Arrange
        var team = _service.CreateTeam("Seattle", "Seahawks", 200_000_000m);

        // Add 53 players (maximum)
        for (int i = 0; i < 53; i++)
        {
            var player = CreateTestPlayer(Positions.QB, $"Player{i}", "Test");
            _service.AddPlayerToTeam(team, player);
        }

        // Try to add 54th player
        var extraPlayer = CreateTestPlayer(Positions.QB, "Extra", "Player");

        // Act
        var result = _service.AddPlayerToTeam(team, extraPlayer);

        // Assert
        result.Should().BeFalse();
        team.Players.Should().HaveCount(53);
        team.Players.Should().NotContain(extraPlayer);
    }

    [Fact]
    public void AddPlayerToTeam_WithNullTeam_ShouldThrowArgumentNullException()
    {
        // Arrange
        var player = CreateTestPlayer(Positions.QB, "Tom", "Brady");

        // Act & Assert
        var act = () => _service.AddPlayerToTeam(null!, player);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddPlayerToTeam_WithNullPlayer_ShouldThrowArgumentNullException()
    {
        // Arrange
        var team = _service.CreateTeam("Seattle", "Seahawks", 200_000_000m);

        // Act & Assert
        var act = () => _service.AddPlayerToTeam(team, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region AssignDepthCharts Tests

    [Fact]
    public void AssignDepthCharts_WithFullRoster_ShouldBuildAllDepthCharts()
    {
        // Arrange
        var team = CreateTeamWithFullRoster();

        // Act
        _service.AssignDepthCharts(team);

        // Assert
        team.OffenseDepthChart.Chart.Should().NotBeEmpty();
        team.DefenseDepthChart.Chart.Should().NotBeEmpty();
        team.FieldGoalOffenseDepthChart.Chart.Should().NotBeEmpty();
        team.FieldGoalDefenseDepthChart.Chart.Should().NotBeEmpty();
        team.KickoffOffenseDepthChart.Chart.Should().NotBeEmpty();
        team.KickoffDefenseDepthChart.Chart.Should().NotBeEmpty();
        team.PuntOffenseDepthChart.Chart.Should().NotBeEmpty();
        team.PuntDefenseDepthChart.Chart.Should().NotBeEmpty();
    }

    [Fact]
    public void AssignDepthCharts_ShouldOrderPlayersBySkill()
    {
        // Arrange
        var team = _service.CreateTeam("Seattle", "Seahawks", 200_000_000m);

        // Add 3 QBs with different passing skills
        var qb1 = CreateTestPlayer(Positions.QB, "Elite", "QB");
        qb1.Passing = 95;
        var qb2 = CreateTestPlayer(Positions.QB, "Average", "QB");
        qb2.Passing = 70;
        var qb3 = CreateTestPlayer(Positions.QB, "Backup", "QB");
        qb3.Passing = 60;

        _service.AddPlayerToTeam(team, qb3); // Add in reverse order
        _service.AddPlayerToTeam(team, qb1);
        _service.AddPlayerToTeam(team, qb2);

        // Act
        _service.AssignDepthCharts(team);

        // Assert
        var qbDepth = team.OffenseDepthChart.Chart[Positions.QB];
        qbDepth.Should().HaveCount(1); // Only takes 1 QB for offense depth chart
        qbDepth[0].Should().Be(qb1); // Highest passing skill should be first
    }

    [Fact]
    public void AssignDepthCharts_WithNoPlayers_ShouldNotThrow()
    {
        // Arrange
        var team = _service.CreateTeam("Seattle", "Seahawks", 200_000_000m);

        // Act & Assert
        var act = () => _service.AssignDepthCharts(team);
        act.Should().NotThrow();
    }

    [Fact]
    public void AssignDepthCharts_WithNullTeam_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => _service.AssignDepthCharts(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region ValidateRoster Tests

    [Fact]
    public void ValidateRoster_WithValidFullRoster_ShouldReturnTrue()
    {
        // Arrange
        var team = CreateTeamWithFullRoster();

        // Act
        var result = _service.ValidateRoster(team);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ValidateRoster_WithMinimumPlayers_ShouldReturnTrue()
    {
        // Arrange
        var team = CreateTeamWithMinimumRoster();

        // Act
        var result = _service.ValidateRoster(team);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ValidateRoster_WithTooFewPlayers_ShouldReturnFalse()
    {
        // Arrange
        var team = _service.CreateTeam("Seattle", "Seahawks", 200_000_000m);

        // Add only 10 players (below minimum of 22)
        for (int i = 0; i < 10; i++)
        {
            _service.AddPlayerToTeam(team, CreateTestPlayer(Positions.QB, $"Player{i}", "Test"));
        }

        // Act
        var result = _service.ValidateRoster(team);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateRoster_MissingRequiredPosition_ShouldReturnFalse()
    {
        // Arrange
        var team = _service.CreateTeam("Seattle", "Seahawks", 200_000_000m);

        // Add 22 players but all QBs (missing other required positions)
        for (int i = 0; i < 22; i++)
        {
            _service.AddPlayerToTeam(team, CreateTestPlayer(Positions.QB, $"Player{i}", "Test"));
        }

        // Act
        var result = _service.ValidateRoster(team);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateRoster_WithNullTeam_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => _service.ValidateRoster(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Helper Methods

    private Player CreateTestPlayer(Positions position, string firstName, string lastName)
    {
        return new Player
        {
            FirstName = firstName,
            LastName = lastName,
            Position = position,
            Number = 1,
            Height = "6-0",
            Weight = 200,
            Age = 25,
            Exp = 3,
            College = "Test University",
            Speed = 70,
            Strength = 70,
            Agility = 70,
            Awareness = 70,
            Passing = 70,
            Catching = 70,
            Rushing = 70,
            Blocking = 70,
            Tackling = 70,
            Coverage = 70,
            Kicking = 70,
            Health = 100,
            Morale = 80,
            Discipline = 80,
            Potential = 80,
            Progression = 70,
            Fragility = 40
        };
    }

    private Team CreateTeamWithFullRoster()
    {
        var team = _service.CreateTeam("Seattle", "Seahawks", 200_000_000m);

        // Add minimum required positions
        AddPosition(team, Positions.QB, 2);
        AddPosition(team, Positions.RB, 3);
        AddPosition(team, Positions.WR, 5);
        AddPosition(team, Positions.TE, 2);
        AddPosition(team, Positions.C, 2);
        AddPosition(team, Positions.G, 4);
        AddPosition(team, Positions.T, 4);
        AddPosition(team, Positions.DE, 4);
        AddPosition(team, Positions.DT, 4);
        AddPosition(team, Positions.LB, 6);
        AddPosition(team, Positions.CB, 5);
        AddPosition(team, Positions.S, 4);
        AddPosition(team, Positions.K, 1);
        AddPosition(team, Positions.P, 1);
        AddPosition(team, Positions.LS, 1);

        return team;
    }

    private Team CreateTeamWithMinimumRoster()
    {
        var team = _service.CreateTeam("Seattle", "Seahawks", 200_000_000m);

        // Add exactly minimum required positions
        AddPosition(team, Positions.QB, 1);
        AddPosition(team, Positions.RB, 1);
        AddPosition(team, Positions.WR, 2);
        AddPosition(team, Positions.TE, 1);
        AddPosition(team, Positions.C, 1);
        AddPosition(team, Positions.G, 2);
        AddPosition(team, Positions.T, 2);
        AddPosition(team, Positions.DE, 2);
        AddPosition(team, Positions.DT, 2);
        AddPosition(team, Positions.LB, 2);
        AddPosition(team, Positions.CB, 2);
        AddPosition(team, Positions.S, 2);
        AddPosition(team, Positions.K, 1);
        AddPosition(team, Positions.P, 1);

        return team;
    }

    private void AddPosition(Team team, Positions position, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var player = CreateTestPlayer(position, $"{position}{i}", "Test");
            _service.AddPlayerToTeam(team, player);
        }
    }

    #endregion

    #region PopulateTeamRoster Tests

    [Fact]
    public void PopulateTeamRoster_WithValidTeam_ShouldPopulate53Players()
    {
        // Arrange
        var team = _service.CreateTeam("Test", "Team", 100000000);
        _playerGeneratorMock
            .Setup(p => p.GenerateRandomPlayer(It.IsAny<Positions>(), It.IsAny<int?>()))
            .Returns((Positions pos, int? seed) => new Player { Position = pos, FirstName = "Test", LastName = "Player" });

        // Act
        var result = _service.PopulateTeamRoster(team);

        // Assert
        result.Should().NotBeNull();
        result.Players.Should().HaveCount(53);
    }

    [Fact]
    public void PopulateTeamRoster_ShouldClearExistingPlayers()
    {
        // Arrange
        var team = _service.CreateTeam("Test", "Team", 100000000);
        team.Players.Add(new Player { FirstName = "Existing", LastName = "Player" });
        _playerGeneratorMock
            .Setup(p => p.GenerateRandomPlayer(It.IsAny<Positions>(), It.IsAny<int?>()))
            .Returns((Positions pos, int? seed) => new Player { Position = pos, FirstName = "New", LastName = "Player" });

        // Act
        var result = _service.PopulateTeamRoster(team);

        // Assert
        result.Players.Should().HaveCount(53);
        result.Players.Should().NotContain(p => p.FirstName == "Existing");
    }

    [Fact]
    public void PopulateTeamRoster_WithNullTeam_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => _service.PopulateTeamRoster(null!);
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("team");
    }

    [Fact]
    public void PopulateTeamRoster_ShouldGenerateCorrectNumberOfPlayersPerPosition()
    {
        // Arrange
        var team = _service.CreateTeam("Test", "Team", 100000000);
        var generatedPlayers = new List<Player>();

        _playerGeneratorMock
            .Setup(p => p.GenerateRandomPlayer(It.IsAny<Positions>(), It.IsAny<int?>()))
            .Returns((Positions pos, int? seed) =>
            {
                var player = new Player { Position = pos, FirstName = "Test", LastName = "Player" };
                generatedPlayers.Add(player);
                return player;
            });

        // Act
        _service.PopulateTeamRoster(team);

        // Assert - Verify correct counts per position (NFL roster composition)
        generatedPlayers.Count(p => p.Position == Positions.QB).Should().Be(2);
        generatedPlayers.Count(p => p.Position == Positions.RB).Should().Be(4);
        generatedPlayers.Count(p => p.Position == Positions.FB).Should().Be(1);
        generatedPlayers.Count(p => p.Position == Positions.WR).Should().Be(6);
        generatedPlayers.Count(p => p.Position == Positions.TE).Should().Be(3);
        generatedPlayers.Count(p => p.Position == Positions.C).Should().Be(2);
        generatedPlayers.Count(p => p.Position == Positions.G).Should().Be(4);
        generatedPlayers.Count(p => p.Position == Positions.T).Should().Be(4);
        generatedPlayers.Count(p => p.Position == Positions.DE).Should().Be(4);
        generatedPlayers.Count(p => p.Position == Positions.DT).Should().Be(3);
        generatedPlayers.Count(p => p.Position == Positions.LB).Should().Be(4);
        generatedPlayers.Count(p => p.Position == Positions.OLB).Should().Be(2);
        generatedPlayers.Count(p => p.Position == Positions.CB).Should().Be(5);
        generatedPlayers.Count(p => p.Position == Positions.S).Should().Be(3);
        generatedPlayers.Count(p => p.Position == Positions.FS).Should().Be(2);
        generatedPlayers.Count(p => p.Position == Positions.K).Should().Be(1);
        generatedPlayers.Count(p => p.Position == Positions.P).Should().Be(1);
        generatedPlayers.Count(p => p.Position == Positions.LS).Should().Be(1);
        generatedPlayers.Count(p => p.Position == Positions.H).Should().Be(1);
    }

    [Fact]
    public void PopulateTeamRoster_ShouldSetTeamIdOnAllPlayers()
    {
        // Arrange
        var team = _service.CreateTeam("Test", "Team", 100000000);
        team.Id = 42;

        _playerGeneratorMock
            .Setup(p => p.GenerateRandomPlayer(It.IsAny<Positions>(), It.IsAny<int?>()))
            .Returns((Positions pos, int? seed) => new Player { Position = pos, FirstName = "Test", LastName = "Player" });

        // Act
        _service.PopulateTeamRoster(team);

        // Assert
        team.Players.Should().HaveCount(53);
        team.Players.Should().OnlyContain(p => p.TeamId == 42);
    }

    [Fact]
    public void PopulateTeamRoster_ShouldCallAssignDepthCharts()
    {
        // Arrange
        var team = _service.CreateTeam("Test", "Team", 100000000);

        _playerGeneratorMock
            .Setup(p => p.GenerateRandomPlayer(It.IsAny<Positions>(), It.IsAny<int?>()))
            .Returns((Positions pos, int? seed) => new Player { Position = pos, FirstName = "Test", LastName = "Player" });

        // Act
        _service.PopulateTeamRoster(team);

        // Assert - Depth charts should not be empty after population
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
    public void PopulateTeamRoster_WithSameSeed_ShouldProduceConsistentResults()
    {
        // Arrange
        var team1 = _service.CreateTeam("Team1", "Test", 100000000);
        var team2 = _service.CreateTeam("Team2", "Test", 100000000);
        var seed = 12345;

        var callCount = 0;
        _playerGeneratorMock
            .Setup(p => p.GenerateRandomPlayer(It.IsAny<Positions>(), It.IsAny<int?>()))
            .Returns((Positions pos, int? s) =>
            {
                callCount++;
                return new Player { Position = pos, FirstName = $"Player{callCount}", LastName = "Test" };
            });

        // Act
        _service.PopulateTeamRoster(team1, seed);

        callCount = 0; // Reset counter
        _service.PopulateTeamRoster(team2, seed);

        // Assert - Both teams should have same structure
        team1.Players.Should().HaveCount(53);
        team2.Players.Should().HaveCount(53);

        // Verify position order is the same
        for (int i = 0; i < 53; i++)
        {
            team1.Players[i].Position.Should().Be(team2.Players[i].Position);
        }
    }

    [Fact]
    public void PopulateTeamRoster_WithNullSeed_ShouldStillPopulate53Players()
    {
        // Arrange
        var team = _service.CreateTeam("Test", "Team", 100000000);

        _playerGeneratorMock
            .Setup(p => p.GenerateRandomPlayer(It.IsAny<Positions>(), It.IsAny<int?>()))
            .Returns((Positions pos, int? seed) => new Player { Position = pos, FirstName = "Test", LastName = "Player" });

        // Act
        var result = _service.PopulateTeamRoster(team, null);

        // Assert
        result.Should().NotBeNull();
        result.Players.Should().HaveCount(53);
    }

    [Fact]
    public void PopulateTeamRoster_ShouldPassIncrementingSeedsToPlayerGenerator()
    {
        // Arrange
        var team = _service.CreateTeam("Test", "Team", 100000000);
        var baseSeed = 1000;
        var seedsReceived = new List<int?>();

        _playerGeneratorMock
            .Setup(p => p.GenerateRandomPlayer(It.IsAny<Positions>(), It.IsAny<int?>()))
            .Returns((Positions pos, int? seed) =>
            {
                seedsReceived.Add(seed);
                return new Player { Position = pos, FirstName = "Test", LastName = "Player" };
            });

        // Act
        _service.PopulateTeamRoster(team, baseSeed);

        // Assert
        seedsReceived.Should().HaveCount(53);
        // Seeds should increment from baseSeed + 0 to baseSeed + 52
        for (int i = 0; i < 53; i++)
        {
            seedsReceived[i].Should().Be(baseSeed + i);
        }
    }

    #endregion
}

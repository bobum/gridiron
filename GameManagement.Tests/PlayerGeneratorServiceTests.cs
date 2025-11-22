using DataAccessLayer.Repositories;
using DomainObjects;
using FluentAssertions;
using GameManagement.Services;
using GameManagement.Tests.TestHelpers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GameManagement.Tests;

/// <summary>
/// Comprehensive tests for PlayerGeneratorService
/// </summary>
public class PlayerGeneratorServiceTests
{
    private readonly Mock<ILogger<PlayerGeneratorService>> _loggerMock;
    private readonly IPlayerDataRepository _playerDataRepository;
    private readonly PlayerGeneratorService _service;

    public PlayerGeneratorServiceTests()
    {
        _loggerMock = new Mock<ILogger<PlayerGeneratorService>>();

        // Use JSON repository for tests
        var testDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "TestData");
        _playerDataRepository = new JsonPlayerDataRepository(testDataPath);

        _service = new PlayerGeneratorService(_loggerMock.Object, _playerDataRepository);
    }

    #region GenerateRandomPlayer Tests

    [Theory]
    [InlineData(Positions.QB)]
    [InlineData(Positions.RB)]
    [InlineData(Positions.WR)]
    [InlineData(Positions.TE)]
    [InlineData(Positions.T)]
    [InlineData(Positions.G)]
    [InlineData(Positions.C)]
    [InlineData(Positions.DE)]
    [InlineData(Positions.DT)]
    [InlineData(Positions.LB)]
    [InlineData(Positions.CB)]
    [InlineData(Positions.S)]
    [InlineData(Positions.K)]
    [InlineData(Positions.P)]
    public void GenerateRandomPlayer_ShouldCreatePlayerWithCorrectPosition(Positions position)
    {
        // Act
        var player = _service.GenerateRandomPlayer(position);

        // Assert
        player.Should().NotBeNull();
        player.Position.Should().Be(position);
    }

    [Fact]
    public void GenerateRandomPlayer_ShouldHaveValidAttributes()
    {
        // Act
        var player = _service.GenerateRandomPlayer(Positions.QB);

        // Assert
        player.FirstName.Should().NotBeNullOrEmpty();
        player.LastName.Should().NotBeNullOrEmpty();
        player.College.Should().NotBeNullOrEmpty();
        player.Number.Should().BeGreaterThan(0);
        player.Height.Should().NotBeNullOrEmpty();
        player.Weight.Should().BeGreaterThan(0);
        player.Age.Should().BeInRange(22, 30);
        player.Exp.Should().BeInRange(0, 11);
    }

    [Fact]
    public void GenerateRandomPlayer_ShouldHaveAttributesInValidRange()
    {
        // Act
        var player = _service.GenerateRandomPlayer(Positions.QB);

        // Assert
        player.Speed.Should().BeInRange(60, 95);
        player.Strength.Should().BeInRange(60, 95);
        player.Agility.Should().BeInRange(60, 95);
        player.Awareness.Should().BeInRange(60, 95);
        player.Passing.Should().BeInRange(60, 95);
        player.Health.Should().Be(100);
        player.Morale.Should().BeInRange(70, 100);
        player.Discipline.Should().BeInRange(60, 95);
    }

    [Theory]
    [InlineData(Positions.QB)]
    [InlineData(Positions.RB)]
    [InlineData(Positions.WR)]
    [InlineData(Positions.TE)]
    [InlineData(Positions.T)]
    [InlineData(Positions.G)]
    [InlineData(Positions.C)]
    [InlineData(Positions.DE)]
    [InlineData(Positions.DT)]
    [InlineData(Positions.LB)]
    [InlineData(Positions.OLB)]
    [InlineData(Positions.CB)]
    [InlineData(Positions.S)]
    [InlineData(Positions.FS)]
    [InlineData(Positions.K)]
    [InlineData(Positions.P)]
    public void GenerateRandomPlayer_AllAttributes_ShouldBeWithinValidRanges(Positions position)
    {
        // Act
        var player = _service.GenerateRandomPlayer(position);

        // Assert - Core Physical Attributes (60-95 for veterans)
        player.Speed.Should().BeInRange(60, 95, "Speed should be within veteran range");
        player.Strength.Should().BeInRange(60, 95, "Strength should be within veteran range");
        player.Agility.Should().BeInRange(60, 95, "Agility should be within veteran range");
        player.Awareness.Should().BeInRange(60, 95, "Awareness should be within veteran range");

        // Assert - Position-Specific Skill Attributes (60-95 for veterans)
        player.Passing.Should().BeInRange(60, 95, "Passing should be within veteran range");
        player.Catching.Should().BeInRange(60, 95, "Catching should be within veteran range");
        player.Rushing.Should().BeInRange(60, 95, "Rushing should be within veteran range");
        player.Blocking.Should().BeInRange(60, 95, "Blocking should be within veteran range");
        player.Tackling.Should().BeInRange(60, 95, "Tackling should be within veteran range");
        player.Coverage.Should().BeInRange(60, 95, "Coverage should be within veteran range");
        player.Kicking.Should().BeInRange(60, 95, "Kicking should be within veteran range");

        // Assert - Development & Mental Attributes
        player.Potential.Should().BeInRange(60, 90, "Potential should be within veteran range");
        player.Progression.Should().BeInRange(50, 85, "Progression should be within veteran range");
        player.Discipline.Should().BeInRange(60, 95, "Discipline should be within veteran range");

        // Assert - Status Attributes
        player.Health.Should().Be(100, "New players should start at full health");
        player.Morale.Should().BeInRange(70, 100, "Morale should be within valid range");
        player.Fragility.Should().BeInRange(20, 60, "Fragility should be within valid range");

        // Assert - All attributes should be positive numbers
        player.Speed.Should().BeGreaterThan(0, "Speed must be positive");
        player.Strength.Should().BeGreaterThan(0, "Strength must be positive");
        player.Agility.Should().BeGreaterThan(0, "Agility must be positive");
        player.Awareness.Should().BeGreaterThan(0, "Awareness must be positive");
        player.Passing.Should().BeGreaterThan(0, "Passing must be positive");
        player.Catching.Should().BeGreaterThan(0, "Catching must be positive");
        player.Rushing.Should().BeGreaterThan(0, "Rushing must be positive");
        player.Blocking.Should().BeGreaterThan(0, "Blocking must be positive");
        player.Tackling.Should().BeGreaterThan(0, "Tackling must be positive");
        player.Coverage.Should().BeGreaterThan(0, "Coverage must be positive");
        player.Kicking.Should().BeGreaterThan(0, "Kicking must be positive");
        player.Potential.Should().BeGreaterThan(0, "Potential must be positive");
        player.Progression.Should().BeGreaterThan(0, "Progression must be positive");
        player.Discipline.Should().BeGreaterThan(0, "Discipline must be positive");
        player.Morale.Should().BeGreaterThan(0, "Morale must be positive");
        player.Fragility.Should().BeGreaterThan(0, "Fragility must be positive");
    }

    [Fact]
    public void GenerateRandomPlayer_NoAttribute_ShouldExceedMaximumValue()
    {
        // Act - Generate multiple players to test for edge cases
        var players = new List<Player>();
        for (int i = 0; i < 100; i++)
        {
            players.Add(_service.GenerateRandomPlayer(Positions.QB));
            players.Add(_service.GenerateRandomPlayer(Positions.WR));
            players.Add(_service.GenerateRandomPlayer(Positions.CB));
        }

        // Assert - No attribute should exceed its maximum value (95 for most skills)
        players.Should().OnlyContain(p => p.Speed <= 95, "Speed should never exceed 95");
        players.Should().OnlyContain(p => p.Strength <= 95, "Strength should never exceed 95");
        players.Should().OnlyContain(p => p.Agility <= 95, "Agility should never exceed 95");
        players.Should().OnlyContain(p => p.Awareness <= 95, "Awareness should never exceed 95");
        players.Should().OnlyContain(p => p.Passing <= 95, "Passing should never exceed 95");
        players.Should().OnlyContain(p => p.Catching <= 95, "Catching should never exceed 95");
        players.Should().OnlyContain(p => p.Rushing <= 95, "Rushing should never exceed 95");
        players.Should().OnlyContain(p => p.Blocking <= 95, "Blocking should never exceed 95");
        players.Should().OnlyContain(p => p.Tackling <= 95, "Tackling should never exceed 95");
        players.Should().OnlyContain(p => p.Coverage <= 95, "Coverage should never exceed 95");
        players.Should().OnlyContain(p => p.Kicking <= 95, "Kicking should never exceed 95");
        players.Should().OnlyContain(p => p.Potential <= 90, "Potential should never exceed 90 for veterans");
        players.Should().OnlyContain(p => p.Progression <= 85, "Progression should never exceed 85 for veterans");
        players.Should().OnlyContain(p => p.Discipline <= 95, "Discipline should never exceed 95");
        players.Should().OnlyContain(p => p.Morale <= 100, "Morale should never exceed 100");
        players.Should().OnlyContain(p => p.Fragility <= 60, "Fragility should never exceed 60");
        players.Should().OnlyContain(p => p.Health <= 100, "Health should never exceed 100");
    }

    [Fact]
    public void GenerateRandomPlayer_NoAttribute_ShouldBeBelowMinimumValue()
    {
        // Act - Generate multiple players to test for edge cases
        var players = new List<Player>();
        for (int i = 0; i < 100; i++)
        {
            players.Add(_service.GenerateRandomPlayer(Positions.RB));
            players.Add(_service.GenerateRandomPlayer(Positions.LB));
            players.Add(_service.GenerateRandomPlayer(Positions.K));
        }

        // Assert - No attribute should be below its minimum value (60 for most veteran skills)
        players.Should().OnlyContain(p => p.Speed >= 60, "Speed should never be below 60");
        players.Should().OnlyContain(p => p.Strength >= 60, "Strength should never be below 60");
        players.Should().OnlyContain(p => p.Agility >= 60, "Agility should never be below 60");
        players.Should().OnlyContain(p => p.Awareness >= 60, "Awareness should never be below 60");
        players.Should().OnlyContain(p => p.Passing >= 60, "Passing should never be below 60");
        players.Should().OnlyContain(p => p.Catching >= 60, "Catching should never be below 60");
        players.Should().OnlyContain(p => p.Rushing >= 60, "Rushing should never be below 60");
        players.Should().OnlyContain(p => p.Blocking >= 60, "Blocking should never be below 60");
        players.Should().OnlyContain(p => p.Tackling >= 60, "Tackling should never be below 60");
        players.Should().OnlyContain(p => p.Coverage >= 60, "Coverage should never be below 60");
        players.Should().OnlyContain(p => p.Kicking >= 60, "Kicking should never be below 60");
        players.Should().OnlyContain(p => p.Potential >= 60, "Potential should never be below 60");
        players.Should().OnlyContain(p => p.Progression >= 50, "Progression should never be below 50");
        players.Should().OnlyContain(p => p.Discipline >= 60, "Discipline should never be below 60");
        players.Should().OnlyContain(p => p.Morale >= 70, "Morale should never be below 70");
        players.Should().OnlyContain(p => p.Fragility >= 20, "Fragility should never be below 20");
    }

    [Fact]
    public void GenerateRandomPlayer_QB_ShouldHaveHighPassingAttribute()
    {
        // Act
        var player = _service.GenerateRandomPlayer(Positions.QB);

        // Assert
        player.Passing.Should().BeGreaterOrEqualTo(70); // QBs should have high passing
    }

    [Fact]
    public void GenerateRandomPlayer_WR_ShouldHaveHighCatchingAndSpeed()
    {
        // Act
        var player = _service.GenerateRandomPlayer(Positions.WR);

        // Assert
        player.Catching.Should().BeGreaterOrEqualTo(70);
        player.Speed.Should().BeGreaterOrEqualTo(75);
    }

    [Fact]
    public void GenerateRandomPlayer_RB_ShouldHaveHighRushingAndSpeed()
    {
        // Act
        var player = _service.GenerateRandomPlayer(Positions.RB);

        // Assert
        player.Rushing.Should().BeGreaterOrEqualTo(70);
        player.Speed.Should().BeGreaterOrEqualTo(70);
    }

    [Fact]
    public void GenerateRandomPlayer_OL_ShouldHaveHighBlockingAndStrength()
    {
        // Arrange - Test all offensive line positions
        var positions = new[] { Positions.C, Positions.G, Positions.T };

        foreach (var position in positions)
        {
            // Act
            var player = _service.GenerateRandomPlayer(position);

            // Assert
            player.Blocking.Should().BeGreaterOrEqualTo(70, $"because {position} needs high blocking");
            player.Strength.Should().BeGreaterOrEqualTo(70, $"because {position} needs high strength");
        }
    }

    [Fact]
    public void GenerateRandomPlayer_DL_ShouldHaveHighTacklingAndStrength()
    {
        // Arrange
        var positions = new[] { Positions.DE, Positions.DT };

        foreach (var position in positions)
        {
            // Act
            var player = _service.GenerateRandomPlayer(position);

            // Assert
            player.Tackling.Should().BeGreaterOrEqualTo(70, $"because {position} needs high tackling");
            player.Strength.Should().BeGreaterOrEqualTo(70, $"because {position} needs high strength");
        }
    }

    [Fact]
    public void GenerateRandomPlayer_CB_ShouldHaveHighCoverageAndSpeed()
    {
        // Act
        var player = _service.GenerateRandomPlayer(Positions.CB);

        // Assert
        player.Coverage.Should().BeGreaterOrEqualTo(75);
        player.Speed.Should().BeGreaterOrEqualTo(75);
    }

    [Fact]
    public void GenerateRandomPlayer_Kicker_ShouldHaveHighKicking()
    {
        // Act
        var player = _service.GenerateRandomPlayer(Positions.K);

        // Assert
        player.Kicking.Should().BeGreaterOrEqualTo(70);
    }

    [Fact]
    public void GenerateRandomPlayer_WithSeed_ShouldBeReproducible()
    {
        // Arrange
        const int seed = 12345;

        // Act
        var player1 = _service.GenerateRandomPlayer(Positions.QB, seed);
        var player2 = _service.GenerateRandomPlayer(Positions.QB, seed);

        // Assert - Same seed should produce identical players
        player1.FirstName.Should().Be(player2.FirstName);
        player1.LastName.Should().Be(player2.LastName);
        player1.Speed.Should().Be(player2.Speed);
        player1.Passing.Should().Be(player2.Passing);
        player1.Age.Should().Be(player2.Age);
    }

    [Fact]
    public void GenerateRandomPlayer_WithoutSeed_ShouldProduceDifferentPlayers()
    {
        // Act
        var player1 = _service.GenerateRandomPlayer(Positions.QB);
        var player2 = _service.GenerateRandomPlayer(Positions.QB);

        // Assert - Different players should have some differences
        bool areDifferent = player1.FirstName != player2.FirstName ||
                           player1.LastName != player2.LastName ||
                           player1.Speed != player2.Speed ||
                           player1.Passing != player2.Passing;

        areDifferent.Should().BeTrue();
    }

    [Fact]
    public void GenerateRandomPlayer_ShouldHaveSalaryBasedOnRating()
    {
        // Act
        var player = _service.GenerateRandomPlayer(Positions.QB);

        // Assert
        player.Salary.Should().BeGreaterThan(0);
        // QBs should have higher base salary
        player.Salary.Should().BeGreaterThan(1000); // Minimum salary threshold
    }

    #endregion

    #region GenerateDraftClass Tests

    [Fact]
    public void GenerateDraftClass_With7Rounds_ShouldGenerateCorrectNumberOfPlayers()
    {
        // Arrange
        const int year = 2024;
        const int rounds = 7;
        const int expectedCount = 7 * 32; // 7 rounds × 32 teams = 224 players

        // Act
        var draftClass = _service.GenerateDraftClass(year, rounds);

        // Assert
        draftClass.Should().HaveCount(expectedCount);
    }

    [Theory]
    [InlineData(2024, 1, 32)]
    [InlineData(2024, 3, 96)]
    [InlineData(2024, 5, 160)]
    [InlineData(2024, 7, 224)]
    public void GenerateDraftClass_WithVariousRounds_ShouldGenerateCorrectCount(int year, int rounds, int expectedCount)
    {
        // Act
        var draftClass = _service.GenerateDraftClass(year, rounds);

        // Assert
        draftClass.Should().HaveCount(expectedCount);
    }

    [Fact]
    public void GenerateDraftClass_ShouldContainVarietyOfPositions()
    {
        // Act
        var draftClass = _service.GenerateDraftClass(2024, 7);

        // Assert
        var positions = draftClass.Select(p => p.Position).Distinct().ToList();
        positions.Should().HaveCountGreaterThan(10); // Should have many different positions
        positions.Should().Contain(Positions.QB);
        positions.Should().Contain(Positions.WR);
        positions.Should().Contain(Positions.CB);
    }

    [Fact]
    public void GenerateDraftClass_PlayersShouldBeYoung()
    {
        // Act
        var draftClass = _service.GenerateDraftClass(2024, 7);

        // Assert - Draft prospects should be college age
        draftClass.Should().OnlyContain(p => p.Age >= 21 && p.Age <= 23);
    }

    [Fact]
    public void GenerateDraftClass_PlayersShouldBeRookies()
    {
        // Act
        var draftClass = _service.GenerateDraftClass(2024, 7);

        // Assert
        draftClass.Should().OnlyContain(p => p.Exp == 0);
    }

    [Fact]
    public void GenerateDraftClass_PlayersShouldHaveNoContract()
    {
        // Act
        var draftClass = _service.GenerateDraftClass(2024, 7);

        // Assert
        draftClass.Should().OnlyContain(p => p.ContractYears == 0);
        draftClass.Should().OnlyContain(p => p.Salary == 0);
    }

    [Fact]
    public void GenerateDraftClass_PlayersShouldHaveHighPotential()
    {
        // Act
        var draftClass = _service.GenerateDraftClass(2024, 7);

        // Assert - Rookies should have high potential
        draftClass.Should().OnlyContain(p => p.Potential >= 70 && p.Potential <= 99);
    }

    [Fact]
    public void GenerateDraftClass_PlayersShouldHaveLowerSkills()
    {
        // Act
        var draftClass = _service.GenerateDraftClass(2024, 7);

        // Assert - Draft prospects have lower skills than veterans
        var avgSpeed = draftClass.Average(p => p.Speed);
        avgSpeed.Should().BeLessThan(80); // Average should be lower for rookies
    }

    [Fact]
    public void GenerateDraftClass_WithSameYear_ShouldBeReproducible()
    {
        // Arrange
        const int year = 2024;

        // Act
        var draftClass1 = _service.GenerateDraftClass(year, 7);
        var draftClass2 = _service.GenerateDraftClass(year, 7);

        // Assert - Same year should produce same draft class
        draftClass1.Should().HaveCount(draftClass2.Count);
        draftClass1[0].FirstName.Should().Be(draftClass2[0].FirstName);
        draftClass1[0].LastName.Should().Be(draftClass2[0].LastName);
        draftClass1[0].Speed.Should().Be(draftClass2[0].Speed);
    }

    [Fact]
    public void GenerateDraftClass_WithDifferentYears_ShouldProduceDifferentClasses()
    {
        // Act
        var draftClass2024 = _service.GenerateDraftClass(2024, 7);
        var draftClass2025 = _service.GenerateDraftClass(2025, 7);

        // Assert - Different years should produce different players
        bool areDifferent = draftClass2024[0].FirstName != draftClass2025[0].FirstName ||
                           draftClass2024[0].LastName != draftClass2025[0].LastName;

        areDifferent.Should().BeTrue();
    }

    [Fact]
    public void GenerateDraftClass_AllAttributes_ShouldBeWithinRookieRanges()
    {
        // Act
        var draftClass = _service.GenerateDraftClass(2024, 7);

        // Assert - Rookies have different ranges (40-85) compared to veterans (60-95)
        // Core Physical Attributes
        draftClass.Should().OnlyContain(p => p.Speed >= 40 && p.Speed <= 85, "Speed should be in rookie range");
        draftClass.Should().OnlyContain(p => p.Strength >= 40 && p.Strength <= 85, "Strength should be in rookie range");
        draftClass.Should().OnlyContain(p => p.Agility >= 40 && p.Agility <= 85, "Agility should be in rookie range");
        draftClass.Should().OnlyContain(p => p.Awareness >= 40 && p.Awareness <= 85, "Awareness should be in rookie range");

        // Position-Specific Skills
        draftClass.Should().OnlyContain(p => p.Passing >= 40 && p.Passing <= 85, "Passing should be in rookie range");
        draftClass.Should().OnlyContain(p => p.Catching >= 40 && p.Catching <= 85, "Catching should be in rookie range");
        draftClass.Should().OnlyContain(p => p.Rushing >= 40 && p.Rushing <= 85, "Rushing should be in rookie range");
        draftClass.Should().OnlyContain(p => p.Blocking >= 40 && p.Blocking <= 85, "Blocking should be in rookie range");
        draftClass.Should().OnlyContain(p => p.Tackling >= 40 && p.Tackling <= 85, "Tackling should be in rookie range");
        draftClass.Should().OnlyContain(p => p.Coverage >= 40 && p.Coverage <= 85, "Coverage should be in rookie range");
        draftClass.Should().OnlyContain(p => p.Kicking >= 40 && p.Kicking <= 85, "Kicking should be in rookie range");

        // Development & Mental Attributes (rookies have higher potential)
        draftClass.Should().OnlyContain(p => p.Potential >= 70 && p.Potential <= 99, "Rookies should have high potential");
        draftClass.Should().OnlyContain(p => p.Progression >= 60 && p.Progression <= 95, "Rookies should have high progression");
        draftClass.Should().OnlyContain(p => p.Discipline >= 50 && p.Discipline <= 90, "Discipline should be in rookie range");

        // Status Attributes
        draftClass.Should().OnlyContain(p => p.Health == 100, "All rookies should be at full health");
        draftClass.Should().OnlyContain(p => p.Morale >= 80 && p.Morale <= 100, "Rookies should have high morale");
        draftClass.Should().OnlyContain(p => p.Fragility >= 20 && p.Fragility <= 60, "Fragility should be in valid range");

        // All attributes must be positive
        draftClass.Should().OnlyContain(p => p.Speed > 0, "Speed must be positive");
        draftClass.Should().OnlyContain(p => p.Strength > 0, "Strength must be positive");
        draftClass.Should().OnlyContain(p => p.Passing > 0, "Passing must be positive");
        draftClass.Should().OnlyContain(p => p.Catching > 0, "Catching must be positive");
        draftClass.Should().OnlyContain(p => p.Rushing > 0, "Rushing must be positive");
        draftClass.Should().OnlyContain(p => p.Blocking > 0, "Blocking must be positive");
        draftClass.Should().OnlyContain(p => p.Tackling > 0, "Tackling must be positive");
        draftClass.Should().OnlyContain(p => p.Coverage > 0, "Coverage must be positive");
        draftClass.Should().OnlyContain(p => p.Kicking > 0, "Kicking must be positive");
    }

    #endregion

    #region GenerateMultiplePlayers Tests

    [Theory]
    [InlineData(10)]
    [InlineData(25)]
    [InlineData(50)]
    [InlineData(100)]
    public void GenerateMultiplePlayers_ShouldGenerateCorrectCount(int count)
    {
        // Act
        var players = _service.GenerateMultiplePlayers(count);

        // Assert
        players.Should().HaveCount(count);
    }

    [Fact]
    public void GenerateMultiplePlayers_ShouldContainVarietyOfPositions()
    {
        // Act
        var players = _service.GenerateMultiplePlayers(50);

        // Assert
        var positions = players.Select(p => p.Position).Distinct().ToList();
        positions.Should().HaveCountGreaterThan(5); // Should have variety
    }

    [Fact]
    public void GenerateMultiplePlayers_WithSeed_ShouldBeReproducible()
    {
        // Arrange
        const int seed = 99999;
        const int count = 10;

        // Act
        var players1 = _service.GenerateMultiplePlayers(count, seed);
        var players2 = _service.GenerateMultiplePlayers(count, seed);

        // Assert
        players1[0].FirstName.Should().Be(players2[0].FirstName);
        players1[0].LastName.Should().Be(players2[0].LastName);
        players1[0].Position.Should().Be(players2[0].Position);
    }

    #endregion

    #region Jersey Number Tests

    [Fact]
    public void GenerateRandomPlayer_ShouldAssignPositionAppropriateJerseyNumbers()
    {
        // Act
        var qb = _service.GenerateRandomPlayer(Positions.QB);
        var wr = _service.GenerateRandomPlayer(Positions.WR);
        var ol = _service.GenerateRandomPlayer(Positions.T);
        var kicker = _service.GenerateRandomPlayer(Positions.K);

        // Assert
        qb.Number.Should().BeInRange(1, 19); // QBs typically 1-19
        wr.Number.Should().BeInRange(10, 89); // WRs can be 10-89
        ol.Number.Should().BeInRange(50, 79); // OL typically 50-79
        kicker.Number.Should().BeInRange(1, 19); // Kickers typically 1-19
    }

    #endregion

    #region Height and Weight Tests

    [Fact]
    public void GenerateRandomPlayer_ShouldAssignRealisticHeightAndWeight()
    {
        // Act
        var qb = _service.GenerateRandomPlayer(Positions.QB);
        var ol = _service.GenerateRandomPlayer(Positions.T);
        var cb = _service.GenerateRandomPlayer(Positions.CB);

        // Assert
        qb.Height.Should().NotBeNullOrEmpty();
        qb.Height.Should().Contain("-"); // Format: "6-2"
        qb.Weight.Should().BeInRange(210, 240);

        ol.Weight.Should().BeInRange(300, 340); // OL are heaviest
        cb.Weight.Should().BeInRange(180, 210); // CBs are lighter
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void PlayerGenerator_ShouldSupportFullTeamRosterGeneration()
    {
        // Act - Generate enough players for a full 53-man roster
        var players = _service.GenerateMultiplePlayers(53);

        // Assert
        players.Should().HaveCount(53);
        players.Should().OnlyContain(p => p.FirstName != null);
        players.Should().OnlyContain(p => p.LastName != null);
        players.Should().OnlyContain(p => p.College != null);
    }

    [Fact]
    public void PlayerGenerator_GeneratedPlayersShouldBeGameReady()
    {
        // Act
        var player = _service.GenerateRandomPlayer(Positions.QB);

        // Assert - All required fields should be set
        player.FirstName.Should().NotBeNullOrEmpty();
        player.LastName.Should().NotBeNullOrEmpty();
        player.College.Should().NotBeNullOrEmpty();
        player.Position.Should().Be(Positions.QB); // Should be the requested position
        player.Number.Should().BeGreaterThan(0);
        player.Height.Should().NotBeNullOrEmpty();
        player.Weight.Should().BeGreaterThan(0);
        player.Age.Should().BeGreaterThan(0);
        player.Health.Should().Be(100);
        player.Salary.Should().BeGreaterThan(0);
    }

    #endregion
}

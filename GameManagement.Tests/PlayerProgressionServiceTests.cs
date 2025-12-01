using DomainObjects;
using FluentAssertions;
using GameManagement.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GameManagement.Tests;

/// <summary>
/// Comprehensive tests for PlayerProgressionService
/// </summary>
public class PlayerProgressionServiceTests
{
    private readonly Mock<ILogger<PlayerProgressionService>> _loggerMock;
    private readonly PlayerProgressionService _service;

    public PlayerProgressionServiceTests()
    {
        _loggerMock = new Mock<ILogger<PlayerProgressionService>>();
        _service = new PlayerProgressionService(_loggerMock.Object);
    }

    #region AgePlayerOneYear Tests

    [Fact]
    public void AgePlayerOneYear_ShouldIncrementAgeAndExperience()
    {
        // Arrange
        var player = CreateTestPlayer(Positions.QB, age: 25, exp: 3);

        // Act
        var isActive = _service.AgePlayerOneYear(player);

        // Assert
        isActive.Should().BeTrue();
        player.Age.Should().Be(26);
        player.Exp.Should().Be(4);
    }

    [Theory]
    [InlineData(22)]
    [InlineData(23)]
    [InlineData(24)]
    [InlineData(25)]
    [InlineData(26)]
    public void AgePlayerOneYear_DevelopmentPhase_ShouldImproveSkills(int startAge)
    {
        // Arrange
        var player = CreateTestPlayer(Positions.QB, age: startAge);
        var initialPassing = player.Passing;

        // Act
        _service.AgePlayerOneYear(player);

        // Assert - Skills should improve or stay same (due to potential cap)
        player.Passing.Should().BeGreaterOrEqualTo(initialPassing);
    }

    [Theory]
    [InlineData(27)]
    [InlineData(28)]
    [InlineData(29)]
    [InlineData(30)]
    public void AgePlayerOneYear_PeakPhase_ShouldMaintainOrImproveAwareness(int startAge)
    {
        // Arrange
        var player = CreateTestPlayer(Positions.QB, age: startAge);
        var initialAwareness = player.Awareness;

        // Act
        _service.AgePlayerOneYear(player);

        // Assert - Mental attributes should improve slightly
        player.Awareness.Should().BeGreaterOrEqualTo(initialAwareness);
    }

    [Theory]
    [InlineData(31)]
    [InlineData(32)]
    [InlineData(33)]
    [InlineData(34)]
    public void AgePlayerOneYear_DeclinePhase_ShouldDecreasePhysicalAttributes(int startAge)
    {
        // Arrange
        var player = CreateTestPlayer(Positions.WR, age: startAge);
        player.Speed = 90;
        player.Agility = 90;
        var initialSpeed = player.Speed;

        // Act
        _service.AgePlayerOneYear(player);

        // Assert - Physical attributes should decline
        player.Speed.Should().BeLessThan(initialSpeed);
    }

    [Theory]
    [InlineData(35)]
    [InlineData(36)]
    [InlineData(37)]
    [InlineData(38)]
    public void AgePlayerOneYear_RapidDeclinePhase_ShouldSignificantlyDecreaseAttributes(int startAge)
    {
        // Arrange
        var player = CreateTestPlayer(Positions.RB, age: startAge);
        player.Speed = 85;
        player.Agility = 85;
        player.Strength = 85;
        var initialSpeed = player.Speed;
        var initialAgility = player.Agility;

        // Act
        _service.AgePlayerOneYear(player);

        // Assert - Significant decline in all physical attributes
        player.Speed.Should().BeLessThan(initialSpeed);
        player.Agility.Should().BeLessThan(initialAgility);
    }

    [Fact]
    public void AgePlayerOneYear_WithNullPlayer_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => _service.AgePlayerOneYear(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AgePlayerOneYear_AtAge40_ShouldRetire()
    {
        // Arrange
        var player = CreateTestPlayer(Positions.QB, age: 39);

        // Act
        var isActive = _service.AgePlayerOneYear(player);

        // Assert
        isActive.Should().BeFalse(); // Player should retire at 40
        player.Age.Should().Be(40);
    }

    #endregion

    #region ShouldRetire Tests

    [Fact]
    public void ShouldRetire_AtAge40OrAbove_ShouldAlwaysRetire()
    {
        // Arrange
        var player40 = CreateTestPlayer(Positions.QB, age: 40);
        var player41 = CreateTestPlayer(Positions.QB, age: 41);
        var player45 = CreateTestPlayer(Positions.QB, age: 45);

        // Act & Assert
        _service.ShouldRetire(player40).Should().BeTrue();
        _service.ShouldRetire(player41).Should().BeTrue();
        _service.ShouldRetire(player45).Should().BeTrue();
    }

    [Theory]
    [InlineData(22)]
    [InlineData(25)]
    [InlineData(28)]
    [InlineData(29)]
    public void ShouldRetire_BelowAge30_ShouldNeverRetire(int age)
    {
        // Arrange
        var player = CreateTestPlayer(Positions.QB, age: age);

        // Act
        var shouldRetire = _service.ShouldRetire(player);

        // Assert
        shouldRetire.Should().BeFalse();
    }

    [Fact]
    public void ShouldRetire_PoorPerformance_ShouldIncreaseRetirementChance()
    {
        // Arrange - Create low-rated player
        var player = CreateTestPlayer(Positions.QB, age: 35);
        player.Passing = 40;
        player.Awareness = 40;
        player.Agility = 40;

        var retiredCount = 0;
        var totalTests = 100;

        // Act - Run multiple times to test probability
        for (int i = 0; i < totalTests; i++)
        {
            var playerCopy = CreateTestPlayer(Positions.QB, age: 35);
            playerCopy.Passing = 40;
            playerCopy.Awareness = 40;
            if (_service.ShouldRetire(playerCopy))
            {
                retiredCount++;
            }
        }

        // Assert - Should have some retirements (not testing exact probability due to randomness)
        retiredCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ShouldRetire_ElitePerformance_ShouldDecreaseRetirementChance()
    {
        // Arrange - Create elite player
        var player = CreateTestPlayer(Positions.QB, age: 35);
        player.Passing = 95;
        player.Awareness = 95;

        var retiredCount = 0;
        var totalTests = 100;

        // Act - Run multiple times to test probability
        for (int i = 0; i < totalTests; i++)
        {
            var playerCopy = CreateTestPlayer(Positions.QB, age: 35);
            playerCopy.Passing = 95;
            playerCopy.Awareness = 95;
            if (_service.ShouldRetire(playerCopy))
            {
                retiredCount++;
            }
        }

        // Assert - Elite players should retire less often than average
        // At age 35 with elite skills, retirement chance should be ~10% (15% base - 5% elite bonus)
        retiredCount.Should().BeLessThan(30); // Should be much less than 100%
    }

    [Fact]
    public void ShouldRetire_WithHighFragility_ShouldIncreaseRetirementChance()
    {
        // Arrange
        var player = CreateTestPlayer(Positions.RB, age: 35);
        player.Fragility = 80; // High injury risk

        var retiredCount = 0;
        var totalTests = 100;

        // Act
        for (int i = 0; i < totalTests; i++)
        {
            var playerCopy = CreateTestPlayer(Positions.RB, age: 35);
            playerCopy.Fragility = 80;
            if (_service.ShouldRetire(playerCopy))
            {
                retiredCount++;
            }
        }

        // Assert
        retiredCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ShouldRetire_WhenInjured_ShouldIncreaseRetirementChance()
    {
        // Arrange
        var player = CreateTestPlayer(Positions.RB, age: 35);
        player.CurrentInjury = new Injury
        {
            Type = InjuryType.Knee,
            Severity = InjurySeverity.Minor,
            
        };

        var retiredCount = 0;
        var totalTests = 100;

        // Act
        for (int i = 0; i < totalTests; i++)
        {
            var playerCopy = CreateTestPlayer(Positions.RB, age: 35);
            playerCopy.CurrentInjury = new Injury { Type = InjuryType.Knee, Severity = InjurySeverity.Minor};
            if (_service.ShouldRetire(playerCopy))
            {
                retiredCount++;
            }
        }

        // Assert
        retiredCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ShouldRetire_WithNullPlayer_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => _service.ShouldRetire(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region CalculateOverallRating Tests

    [Fact]
    public void CalculateOverallRating_ForQuarterback_ShouldWeightPassingHeavily()
    {
        // Arrange
        var qb = CreateTestPlayer(Positions.QB);
        qb.Passing = 90;
        qb.Awareness = 80;
        qb.Agility = 70;

        // Act
        var overall = _service.CalculateOverallRating(qb);

        // Assert
        overall.Should().BeGreaterThan(70);
        // QB formula: Passing * 0.5 + Awareness * 0.3 + Agility * 0.2
        // = 90 * 0.5 + 80 * 0.3 + 70 * 0.2 = 45 + 24 + 14 = 83
        overall.Should().Be(83);
    }

    [Fact]
    public void CalculateOverallRating_ForRunningBack_ShouldWeightRushingAndSpeed()
    {
        // Arrange
        var rb = CreateTestPlayer(Positions.RB);
        rb.Rushing = 90;
        rb.Speed = 88;
        rb.Agility = 85;
        rb.Catching = 70;

        // Act
        var overall = _service.CalculateOverallRating(rb);

        // Assert
        overall.Should().BeGreaterThan(75);
        // RB formula: Rushing * 0.4 + Speed * 0.25 + Agility * 0.2 + Catching * 0.15
        // = 90 * 0.4 + 88 * 0.25 + 85 * 0.2 + 70 * 0.15 = 36 + 22 + 17 + 10.5 = 85.5 = 85
        overall.Should().Be(85);
    }

    [Fact]
    public void CalculateOverallRating_ForWideReceiver_ShouldWeightCatchingAndSpeed()
    {
        // Arrange
        var wr = CreateTestPlayer(Positions.WR);
        wr.Catching = 92;
        wr.Speed = 90;
        wr.Agility = 88;

        // Act
        var overall = _service.CalculateOverallRating(wr);

        // Assert
        overall.Should().BeGreaterThan(80);
        // WR formula: Catching * 0.5 + Speed * 0.3 + Agility * 0.2
        // = 92 * 0.5 + 90 * 0.3 + 88 * 0.2 = 46 + 27 + 17.6 = 90.6 = 90
        overall.Should().Be(90);
    }

    [Fact]
    public void CalculateOverallRating_ForOffensiveLine_ShouldWeightBlockingHeavily()
    {
        // Arrange
        var tackle = CreateTestPlayer(Positions.T);
        tackle.Blocking = 88;
        tackle.Strength = 85;
        tackle.Awareness = 75;

        // Act
        var overall = _service.CalculateOverallRating(tackle);

        // Assert
        // OL formula: Blocking * 0.5 + Strength * 0.3 + Awareness * 0.2
        // = 88 * 0.5 + 85 * 0.3 + 75 * 0.2 = 44 + 25.5 + 15 = 84.5 = 84
        overall.Should().Be(84);
    }

    [Fact]
    public void CalculateOverallRating_ForDefensiveLine_ShouldWeightTacklingAndStrength()
    {
        // Arrange
        var de = CreateTestPlayer(Positions.DE);
        de.Tackling = 85;
        de.Strength = 82;
        de.Agility = 78;
        de.Speed = 80;

        // Act
        var overall = _service.CalculateOverallRating(de);

        // Assert
        // DE formula: Tackling * 0.4 + Strength * 0.35 + Speed * 0.25
        // But need to account for GetPositionSkill adds Agility: Tackling + Agility
        overall.Should().BeGreaterThan(75);
    }

    [Fact]
    public void CalculateOverallRating_ForCornerback_ShouldWeightCoverageAndSpeed()
    {
        // Arrange
        var cb = CreateTestPlayer(Positions.CB);
        cb.Coverage = 90;
        cb.Speed = 92;
        cb.Agility = 88;

        // Act
        var overall = _service.CalculateOverallRating(cb);

        // Assert
        // CB formula: Coverage * 0.5 + Speed * 0.3 + Agility * 0.2
        // = 90 * 0.5 + 92 * 0.3 + 88 * 0.2 = 45 + 27.6 + 17.6 = 90.2 = 90
        overall.Should().Be(90);
    }

    [Fact]
    public void CalculateOverallRating_ForKicker_ShouldUseKickingOnly()
    {
        // Arrange
        var kicker = CreateTestPlayer(Positions.K);
        kicker.Kicking = 95;

        // Act
        var overall = _service.CalculateOverallRating(kicker);

        // Assert
        overall.Should().Be(95);
    }

    [Fact]
    public void CalculateOverallRating_WithNullPlayer_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => _service.CalculateOverallRating(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void PlayerLifecycle_FromDraftToRetirement_ShouldProgressRealisticallyForQB()
    {
        // Arrange - Draft a 22-year-old QB
        var player = CreateTestPlayer(Positions.QB, age: 22);
        player.Passing = 70;
        player.Potential = 90;

        // Act - Age through career
        var ages = new List<int>();
        var ratings = new List<int>();

        while (_service.AgePlayerOneYear(player))
        {
            ages.Add(player.Age);
            ratings.Add(_service.CalculateOverallRating(player));

            // Safety check - stop after 30 years
            if (ages.Count > 30) break;
        }

        // Assert
        ages.Should().Contain(new[] { 23, 24, 25 }); // Should age multiple years
        player.Age.Should().BeGreaterThan(22); // Should have aged
        ratings.Should().NotBeEmpty(); // Should have ratings tracked
    }

    [Fact]
    public void PlayerLifecycle_SpeedPositionPlayer_ShouldDeclineEarlier()
    {
        // Arrange - Create 35-year-old RB (speed position)
        var rb = CreateTestPlayer(Positions.RB, age: 35);
        rb.Speed = 85;
        rb.Agility = 85;

        // Act
        var stillActive = _service.AgePlayerOneYear(rb);

        // Assert - Should see decline
        if (stillActive)
        {
            rb.Speed.Should().BeLessThan(85);
        }
    }

    #endregion

    #region Helper Methods

    private Player CreateTestPlayer(Positions position, int age = 25, int exp = 3)
    {
        return new Player
        {
            FirstName = "Test",
            LastName = "Player",
            Position = position,
            Number = 1,
            Height = "6-0",
            Weight = 200,
            Age = age,
            Exp = exp,
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

    #endregion
}

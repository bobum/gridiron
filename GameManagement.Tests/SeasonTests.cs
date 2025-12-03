using DomainObjects;
using FluentAssertions;
using Xunit;

namespace GameManagement.Tests;

/// <summary>
/// Unit tests for Season entity business logic
/// Tests AdvanceWeek and AdvancePhase methods.
/// </summary>
public class SeasonTests
{
    #region Constructor / Default State Tests

    [Fact]
    public void Season_WhenCreated_ShouldHaveDefaultValues()
    {
        // Act
        var season = new Season();

        // Assert
        season.CurrentWeek.Should().Be(1);
        season.Phase.Should().Be(SeasonPhase.Preseason);
        season.IsComplete.Should().BeFalse();
        season.RegularSeasonWeeks.Should().Be(17);
        season.Weeks.Should().NotBeNull().And.BeEmpty();
    }

    #endregion

    #region AdvanceWeek Tests - Preseason

    [Theory]
    [InlineData(1, 2)]
    [InlineData(2, 3)]
    [InlineData(3, 4)]
    public void AdvanceWeek_InPreseason_WhenNotAtLastWeek_ShouldAdvanceAndReturnTrue(
        int currentWeek, int expectedWeek)
    {
        // Arrange
        var season = new Season
        {
            Phase = SeasonPhase.Preseason,
            CurrentWeek = currentWeek
        };

        // Act
        var result = season.AdvanceWeek();

        // Assert
        result.Should().BeTrue();
        season.CurrentWeek.Should().Be(expectedWeek);
    }

    [Fact]
    public void AdvanceWeek_InPreseason_WhenAtLastWeek_ShouldReturnFalse()
    {
        // Arrange
        var season = new Season
        {
            Phase = SeasonPhase.Preseason,
            CurrentWeek = 4 // Last preseason week
        };

        // Act
        var result = season.AdvanceWeek();

        // Assert
        result.Should().BeFalse();
        season.CurrentWeek.Should().Be(4); // Should not change
    }

    #endregion

    #region AdvanceWeek Tests - Regular Season

    [Theory]
    [InlineData(1, 2)]
    [InlineData(8, 9)]
    [InlineData(16, 17)]
    public void AdvanceWeek_InRegularSeason_WhenNotAtLastWeek_ShouldAdvanceAndReturnTrue(
        int currentWeek, int expectedWeek)
    {
        // Arrange
        var season = new Season
        {
            Phase = SeasonPhase.RegularSeason,
            CurrentWeek = currentWeek,
            RegularSeasonWeeks = 17
        };

        // Act
        var result = season.AdvanceWeek();

        // Assert
        result.Should().BeTrue();
        season.CurrentWeek.Should().Be(expectedWeek);
    }

    [Fact]
    public void AdvanceWeek_InRegularSeason_WhenAtLastWeek_ShouldReturnFalse()
    {
        // Arrange
        var season = new Season
        {
            Phase = SeasonPhase.RegularSeason,
            CurrentWeek = 17,
            RegularSeasonWeeks = 17
        };

        // Act
        var result = season.AdvanceWeek();

        // Assert
        result.Should().BeFalse();
        season.CurrentWeek.Should().Be(17);
    }

    [Theory]
    [InlineData(14, 14)]
    [InlineData(18, 18)]
    public void AdvanceWeek_InRegularSeason_WithCustomWeekCount_ShouldRespectConfiguration(
        int regularSeasonWeeks, int lastWeek)
    {
        // Arrange
        var season = new Season
        {
            Phase = SeasonPhase.RegularSeason,
            CurrentWeek = lastWeek,
            RegularSeasonWeeks = regularSeasonWeeks
        };

        // Act
        var result = season.AdvanceWeek();

        // Assert
        result.Should().BeFalse();
        season.CurrentWeek.Should().Be(lastWeek);
    }

    #endregion

    #region AdvanceWeek Tests - Playoffs

    [Theory]
    [InlineData(1, 2)] // Wild Card to Divisional
    [InlineData(2, 3)] // Divisional to Conference
    [InlineData(3, 4)] // Conference to Championship
    public void AdvanceWeek_InPlayoffs_WhenNotAtChampionship_ShouldAdvanceAndReturnTrue(
        int currentWeek, int expectedWeek)
    {
        // Arrange
        var season = new Season
        {
            Phase = SeasonPhase.Playoffs,
            CurrentWeek = currentWeek
        };

        // Act
        var result = season.AdvanceWeek();

        // Assert
        result.Should().BeTrue();
        season.CurrentWeek.Should().Be(expectedWeek);
    }

    [Fact]
    public void AdvanceWeek_InPlayoffs_WhenAtChampionship_ShouldReturnFalse()
    {
        // Arrange
        var season = new Season
        {
            Phase = SeasonPhase.Playoffs,
            CurrentWeek = 4 // Championship week
        };

        // Act
        var result = season.AdvanceWeek();

        // Assert
        result.Should().BeFalse();
        season.CurrentWeek.Should().Be(4);
    }

    #endregion

    #region AdvanceWeek Tests - Offseason

    [Fact]
    public void AdvanceWeek_InOffseason_ShouldReturnFalse()
    {
        // Arrange
        var season = new Season
        {
            Phase = SeasonPhase.Offseason,
            CurrentWeek = 1
        };

        // Act
        var result = season.AdvanceWeek();

        // Assert
        result.Should().BeFalse();
        season.CurrentWeek.Should().Be(1);
    }

    #endregion

    #region AdvanceWeek Tests - Complete Season

    [Fact]
    public void AdvanceWeek_WhenSeasonIsComplete_ShouldReturnFalse()
    {
        // Arrange
        var season = new Season
        {
            Phase = SeasonPhase.RegularSeason,
            CurrentWeek = 5,
            IsComplete = true
        };

        // Act
        var result = season.AdvanceWeek();

        // Assert
        result.Should().BeFalse();
        season.CurrentWeek.Should().Be(5); // Should not change
    }

    #endregion

    #region AdvancePhase Tests

    [Fact]
    public void AdvancePhase_FromPreseason_ShouldTransitionToRegularSeason()
    {
        // Arrange
        var season = new Season
        {
            Phase = SeasonPhase.Preseason,
            CurrentWeek = 4
        };

        // Act
        var result = season.AdvancePhase();

        // Assert
        result.Should().BeTrue();
        season.Phase.Should().Be(SeasonPhase.RegularSeason);
        season.CurrentWeek.Should().Be(1); // Reset to week 1
    }

    [Fact]
    public void AdvancePhase_FromRegularSeason_ShouldTransitionToPlayoffs()
    {
        // Arrange
        var season = new Season
        {
            Phase = SeasonPhase.RegularSeason,
            CurrentWeek = 17
        };

        // Act
        var result = season.AdvancePhase();

        // Assert
        result.Should().BeTrue();
        season.Phase.Should().Be(SeasonPhase.Playoffs);
        season.CurrentWeek.Should().Be(1);
    }

    [Fact]
    public void AdvancePhase_FromPlayoffs_ShouldTransitionToOffseason()
    {
        // Arrange
        var season = new Season
        {
            Phase = SeasonPhase.Playoffs,
            CurrentWeek = 4
        };

        // Act
        var result = season.AdvancePhase();

        // Assert
        result.Should().BeTrue();
        season.Phase.Should().Be(SeasonPhase.Offseason);
        season.CurrentWeek.Should().Be(1);
    }

    [Fact]
    public void AdvancePhase_FromPlayoffs_ShouldMarkSeasonComplete()
    {
        // Arrange
        var season = new Season
        {
            Phase = SeasonPhase.Playoffs,
            CurrentWeek = 4
        };

        // Act
        var result = season.AdvancePhase();

        // Assert
        season.IsComplete.Should().BeTrue();
        season.EndDate.Should().NotBeNull();
        season.EndDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void AdvancePhase_FromOffseason_ShouldReturnFalse()
    {
        // Arrange
        var season = new Season
        {
            Phase = SeasonPhase.Offseason,
            CurrentWeek = 1
        };

        // Act
        var result = season.AdvancePhase();

        // Assert
        result.Should().BeFalse();
        season.Phase.Should().Be(SeasonPhase.Offseason);
    }

    [Fact]
    public void AdvancePhase_WhenSeasonIsComplete_ShouldReturnFalse()
    {
        // Arrange
        var season = new Season
        {
            Phase = SeasonPhase.RegularSeason,
            CurrentWeek = 10,
            IsComplete = true
        };

        // Act
        var result = season.AdvancePhase();

        // Assert
        result.Should().BeFalse();
        season.Phase.Should().Be(SeasonPhase.RegularSeason); // Should not change
    }

    #endregion

    #region Full Season Lifecycle Tests

    [Fact]
    public void Season_FullLifecycle_ShouldProgressThroughAllPhases()
    {
        // Arrange
        var season = new Season
        {
            Year = 2024,
            Phase = SeasonPhase.Preseason,
            CurrentWeek = 1,
            RegularSeasonWeeks = 17
        };

        // Act & Assert - Preseason (4 weeks)
        season.Phase.Should().Be(SeasonPhase.Preseason);
        for (int week = 1; week < 4; week++)
        {
            season.AdvanceWeek().Should().BeTrue();
        }

        season.AdvanceWeek().Should().BeFalse(); // Can't advance past week 4
        season.CurrentWeek.Should().Be(4);

        // Transition to Regular Season
        season.AdvancePhase().Should().BeTrue();
        season.Phase.Should().Be(SeasonPhase.RegularSeason);
        season.CurrentWeek.Should().Be(1);

        // Regular Season (17 weeks)
        for (int week = 1; week < 17; week++)
        {
            season.AdvanceWeek().Should().BeTrue();
        }

        season.AdvanceWeek().Should().BeFalse();
        season.CurrentWeek.Should().Be(17);

        // Transition to Playoffs
        season.AdvancePhase().Should().BeTrue();
        season.Phase.Should().Be(SeasonPhase.Playoffs);
        season.CurrentWeek.Should().Be(1);

        // Playoffs (4 weeks)
        for (int week = 1; week < 4; week++)
        {
            season.AdvanceWeek().Should().BeTrue();
        }

        season.AdvanceWeek().Should().BeFalse();
        season.CurrentWeek.Should().Be(4);

        // Season should not be complete yet
        season.IsComplete.Should().BeFalse();

        // Transition to Offseason
        season.AdvancePhase().Should().BeTrue();
        season.Phase.Should().Be(SeasonPhase.Offseason);
        season.IsComplete.Should().BeTrue();
        season.EndDate.Should().NotBeNull();

        // Can't advance further
        season.AdvancePhase().Should().BeFalse();
        season.AdvanceWeek().Should().BeFalse();
    }

    #endregion
}

using DomainObjects;
using FluentAssertions;
using Xunit;

namespace GameManagement.Tests;

/// <summary>
/// Unit tests for SeasonWeek entity business logic
/// Tests DisplayName computed property and IsComplete property.
/// </summary>
public class SeasonWeekTests
{
    #region Constructor / Default State Tests

    [Fact]
    public void SeasonWeek_WhenCreated_ShouldHaveDefaultValues()
    {
        // Act
        var week = new SeasonWeek();

        // Assert
        week.Status.Should().Be(WeekStatus.Scheduled);
        week.Games.Should().NotBeNull().And.BeEmpty();
    }

    #endregion

    #region IsComplete Property Tests

    [Fact]
    public void IsComplete_WhenStatusIsCompleted_ShouldReturnTrue()
    {
        // Arrange
        var week = new SeasonWeek { Status = WeekStatus.Completed };

        // Act & Assert
        week.IsComplete.Should().BeTrue();
    }

    [Theory]
    [InlineData(WeekStatus.Scheduled)]
    [InlineData(WeekStatus.InProgress)]
    public void IsComplete_WhenStatusIsNotCompleted_ShouldReturnFalse(WeekStatus status)
    {
        // Arrange
        var week = new SeasonWeek { Status = status };

        // Act & Assert
        week.IsComplete.Should().BeFalse();
    }

    #endregion

    #region DisplayName Tests - Preseason

    [Theory]
    [InlineData(1, "Preseason Week 1")]
    [InlineData(2, "Preseason Week 2")]
    [InlineData(3, "Preseason Week 3")]
    [InlineData(4, "Preseason Week 4")]
    public void DisplayName_InPreseason_ShouldReturnPreseasonWeekFormat(
        int weekNumber, string expectedName)
    {
        // Arrange
        var week = new SeasonWeek
        {
            Phase = SeasonPhase.Preseason,
            WeekNumber = weekNumber
        };

        // Act & Assert
        week.DisplayName.Should().Be(expectedName);
    }

    #endregion

    #region DisplayName Tests - Regular Season

    [Theory]
    [InlineData(1, "Week 1")]
    [InlineData(5, "Week 5")]
    [InlineData(10, "Week 10")]
    [InlineData(17, "Week 17")]
    [InlineData(18, "Week 18")]
    public void DisplayName_InRegularSeason_ShouldReturnWeekFormat(
        int weekNumber, string expectedName)
    {
        // Arrange
        var week = new SeasonWeek
        {
            Phase = SeasonPhase.RegularSeason,
            WeekNumber = weekNumber
        };

        // Act & Assert
        week.DisplayName.Should().Be(expectedName);
    }

    #endregion

    #region DisplayName Tests - Playoffs

    [Fact]
    public void DisplayName_InPlayoffs_Week1_ShouldReturnWildCardRound()
    {
        // Arrange
        var week = new SeasonWeek
        {
            Phase = SeasonPhase.Playoffs,
            WeekNumber = 1
        };

        // Act & Assert
        week.DisplayName.Should().Be("Wild Card Round");
    }

    [Fact]
    public void DisplayName_InPlayoffs_Week2_ShouldReturnDivisionalRound()
    {
        // Arrange
        var week = new SeasonWeek
        {
            Phase = SeasonPhase.Playoffs,
            WeekNumber = 2
        };

        // Act & Assert
        week.DisplayName.Should().Be("Divisional Round");
    }

    [Fact]
    public void DisplayName_InPlayoffs_Week3_ShouldReturnConferenceChampionships()
    {
        // Arrange
        var week = new SeasonWeek
        {
            Phase = SeasonPhase.Playoffs,
            WeekNumber = 3
        };

        // Act & Assert
        week.DisplayName.Should().Be("Conference Championships");
    }

    [Fact]
    public void DisplayName_InPlayoffs_Week4_ShouldReturnChampionshipGame()
    {
        // Arrange
        var week = new SeasonWeek
        {
            Phase = SeasonPhase.Playoffs,
            WeekNumber = 4
        };

        // Act & Assert
        week.DisplayName.Should().Be("Championship Game");
    }

    [Theory]
    [InlineData(5, "Playoff Round 5")]
    [InlineData(6, "Playoff Round 6")]
    public void DisplayName_InPlayoffs_WeekBeyond4_ShouldReturnPlayoffRoundFormat(
        int weekNumber, string expectedName)
    {
        // Arrange
        var week = new SeasonWeek
        {
            Phase = SeasonPhase.Playoffs,
            WeekNumber = weekNumber
        };

        // Act & Assert
        week.DisplayName.Should().Be(expectedName);
    }

    #endregion

    #region DisplayName Tests - Offseason

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void DisplayName_InOffseason_ShouldReturnOffseason(int weekNumber)
    {
        // Arrange
        var week = new SeasonWeek
        {
            Phase = SeasonPhase.Offseason,
            WeekNumber = weekNumber
        };

        // Act & Assert
        week.DisplayName.Should().Be("Offseason");
    }

    #endregion

    #region DisplayName Tests - Default/Unknown Phase

    [Fact]
    public void DisplayName_WithUnknownPhase_ShouldReturnGenericWeekFormat()
    {
        // Arrange - Use an invalid enum value to test default case
        var week = new SeasonWeek
        {
            Phase = (SeasonPhase)999, // Invalid enum value
            WeekNumber = 5
        };

        // Act & Assert
        week.DisplayName.Should().Be("Week 5");
    }

    #endregion

    #region Status Transition Tests

    [Fact]
    public void SeasonWeek_StatusTransitions_ShouldWorkCorrectly()
    {
        // Arrange
        var week = new SeasonWeek
        {
            Phase = SeasonPhase.RegularSeason,
            WeekNumber = 1
        };

        // Assert initial state
        week.Status.Should().Be(WeekStatus.Scheduled);
        week.IsComplete.Should().BeFalse();

        // Transition to InProgress
        week.Status = WeekStatus.InProgress;
        week.Status.Should().Be(WeekStatus.InProgress);
        week.IsComplete.Should().BeFalse();

        // Transition to Completed
        week.Status = WeekStatus.Completed;
        week.CompletedDate = DateTime.UtcNow;
        week.Status.Should().Be(WeekStatus.Completed);
        week.IsComplete.Should().BeTrue();
        week.CompletedDate.Should().NotBeNull();
    }

    #endregion
}

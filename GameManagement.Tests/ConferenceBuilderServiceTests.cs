using DomainObjects;
using FluentAssertions;
using GameManagement.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GameManagement.Tests;

/// <summary>
/// Comprehensive tests for ConferenceBuilderService
/// </summary>
public class ConferenceBuilderServiceTests
{
    private readonly Mock<ILogger<ConferenceBuilderService>> _loggerMock;
    private readonly ConferenceBuilderService _service;

    public ConferenceBuilderServiceTests()
    {
        _loggerMock = new Mock<ILogger<ConferenceBuilderService>>();
        _service = new ConferenceBuilderService(_loggerMock.Object);
    }

    #region UpdateConference Tests

    [Fact]
    public void UpdateConference_WithValidName_ShouldUpdateName()
    {
        // Arrange
        var conference = new Conference
        {
            Id = 1,
            Name = "Old Conference Name"
        };
        var newName = "New Conference Name";

        // Act
        _service.UpdateConference(conference, newName);

        // Assert
        conference.Name.Should().Be(newName);
    }

    [Fact]
    public void UpdateConference_WithNullConference_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => _service.UpdateConference(null!, "New Name");
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("conference");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void UpdateConference_WithEmptyName_ShouldNotUpdateName(string? emptyName)
    {
        // Arrange
        var conference = new Conference
        {
            Id = 1,
            Name = "Original Name"
        };
        var originalName = conference.Name;

        // Act
        _service.UpdateConference(conference, emptyName);

        // Assert
        conference.Name.Should().Be(originalName);
    }

    [Fact]
    public void UpdateConference_WithWhitespaceName_ShouldNotUpdate()
    {
        // Arrange
        var conference = new Conference
        {
            Id = 1,
            Name = "Original Name"
        };

        // Act
        _service.UpdateConference(conference, "   ");

        // Assert
        conference.Name.Should().Be("Original Name");
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new ConferenceBuilderService(null!);
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("logger");
    }

    #endregion
}

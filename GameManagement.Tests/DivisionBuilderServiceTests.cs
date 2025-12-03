using DomainObjects;
using FluentAssertions;
using GameManagement.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GameManagement.Tests;

/// <summary>
/// Comprehensive tests for DivisionBuilderService.
/// </summary>
public class DivisionBuilderServiceTests
{
    private readonly Mock<ILogger<DivisionBuilderService>> _loggerMock;
    private readonly DivisionBuilderService _service;

    public DivisionBuilderServiceTests()
    {
        _loggerMock = new Mock<ILogger<DivisionBuilderService>>();
        _service = new DivisionBuilderService(_loggerMock.Object);
    }

    #region UpdateDivision Tests

    [Fact]
    public void UpdateDivision_WithValidName_ShouldUpdateName()
    {
        // Arrange
        var division = new Division
        {
            Id = 1,
            Name = "Old Division Name"
        };
        var newName = "New Division Name";

        // Act
        _service.UpdateDivision(division, newName);

        // Assert
        division.Name.Should().Be(newName);
    }

    [Fact]
    public void UpdateDivision_WithNullDivision_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => _service.UpdateDivision(null!, "New Name");
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("division");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void UpdateDivision_WithEmptyName_ShouldNotUpdateName(string? emptyName)
    {
        // Arrange
        var division = new Division
        {
            Id = 1,
            Name = "Original Name"
        };
        var originalName = division.Name;

        // Act
        _service.UpdateDivision(division, emptyName);

        // Assert
        division.Name.Should().Be(originalName);
    }

    [Fact]
    public void UpdateDivision_WithWhitespaceName_ShouldNotUpdate()
    {
        // Arrange
        var division = new Division
        {
            Id = 1,
            Name = "Original Name"
        };

        // Act
        _service.UpdateDivision(division, "   ");

        // Assert
        division.Name.Should().Be("Original Name");
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new DivisionBuilderService(null!);
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("logger");
    }

    #endregion
}

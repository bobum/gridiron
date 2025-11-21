using DataAccessLayer.Repositories;
using DomainObjects;
using FluentAssertions;
using GameManagement.Services;
using Gridiron.WebApi.Controllers;
using Gridiron.WebApi.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Gridiron.WebApi.Tests.Controllers;

/// <summary>
/// Unit tests for TeamsManagementController PopulateTeamRoster endpoint
/// Tests the controller logic WITHOUT touching the database (mocked repositories)
/// </summary>
public class TeamsManagementControllerTests
{
    private readonly Mock<ITeamRepository> _mockTeamRepository;
    private readonly Mock<IPlayerRepository> _mockPlayerRepository;
    private readonly Mock<ITeamBuilderService> _mockTeamBuilderService;
    private readonly Mock<IPlayerGeneratorService> _mockPlayerGeneratorService;
    private readonly Mock<ILogger<TeamsManagementController>> _mockLogger;
    private readonly TeamsManagementController _controller;

    public TeamsManagementControllerTests()
    {
        _mockTeamRepository = new Mock<ITeamRepository>();
        _mockPlayerRepository = new Mock<IPlayerRepository>();
        _mockTeamBuilderService = new Mock<ITeamBuilderService>();
        _mockPlayerGeneratorService = new Mock<IPlayerGeneratorService>();
        _mockLogger = new Mock<ILogger<TeamsManagementController>>();
        _controller = new TeamsManagementController(
            _mockTeamRepository.Object,
            _mockPlayerRepository.Object,
            _mockTeamBuilderService.Object,
            _mockPlayerGeneratorService.Object,
            _mockLogger.Object);
    }

    #region PopulateTeamRoster Tests

    [Fact]
    public async Task PopulateTeamRoster_WhenTeamExists_ReturnsOkWithTeam()
    {
        // Arrange
        var team = CreateTestTeam(1, "City", "Name");
        _mockTeamRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(team);

        _mockTeamBuilderService
            .Setup(s => s.PopulateTeamRoster(team, It.IsAny<int?>()))
            .Returns(team);

        _mockTeamRepository
            .Setup(r => r.UpdateAsync(team))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.PopulateTeamRoster(1, null);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var teamDto = okResult!.Value as TeamDto;
        teamDto!.Id.Should().Be(1);
        teamDto.Name.Should().Be("Name");
        teamDto.City.Should().Be("City");
    }

    [Fact]
    public async Task PopulateTeamRoster_WhenTeamNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockTeamRepository
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Team?)null);

        // Act
        var result = await _controller.PopulateTeamRoster(999, null);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task PopulateTeamRoster_WithSeed_PassesSeedToService()
    {
        // Arrange
        var team = CreateTestTeam(1, "Test", "Team");
        var seed = 54321;

        _mockTeamRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(team);

        _mockTeamBuilderService
            .Setup(s => s.PopulateTeamRoster(team, seed))
            .Returns(team);

        _mockTeamRepository
            .Setup(r => r.UpdateAsync(team))
            .Returns(Task.CompletedTask);

        // Act
        await _controller.PopulateTeamRoster(1, seed);

        // Assert
        _mockTeamBuilderService.Verify(
            s => s.PopulateTeamRoster(team, seed),
            Times.Once);
    }

    [Fact]
    public async Task PopulateTeamRoster_CallsRepositoryUpdate()
    {
        // Arrange
        var team = CreateTestTeam(1, "Test", "Team");

        _mockTeamRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(team);

        _mockTeamBuilderService
            .Setup(s => s.PopulateTeamRoster(team, It.IsAny<int?>()))
            .Returns(team);

        _mockTeamRepository
            .Setup(r => r.UpdateAsync(team))
            .Returns(Task.CompletedTask);

        // Act
        await _controller.PopulateTeamRoster(1, null);

        // Assert
        _mockTeamRepository.Verify(r => r.UpdateAsync(team), Times.Once);
    }

    [Fact]
    public async Task PopulateTeamRoster_WhenServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var team = CreateTestTeam(1, "Test", "Team");

        _mockTeamRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(team);

        _mockTeamBuilderService
            .Setup(s => s.PopulateTeamRoster(It.IsAny<Team>(), It.IsAny<int?>()))
            .Throws(new Exception("Population error"));

        // Act
        var result = await _controller.PopulateTeamRoster(1, null);

        // Assert
        result.Result.Should().BeOfType<ObjectResult>();
        var objectResult = result.Result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }

    #endregion

    #region Helper Methods

    private Team CreateTestTeam(int id, string city, string name)
    {
        return new Team
        {
            Id = id,
            City = city,
            Name = name,
            Players = new List<Player>(),
            Budget = 100000000,
            Championships = 0,
            Wins = 0,
            Losses = 0,
            Ties = 0,
            FanSupport = 50,
            Chemistry = 50,
            OffenseDepthChart = new DepthChart(),
            DefenseDepthChart = new DepthChart(),
            FieldGoalOffenseDepthChart = new DepthChart(),
            FieldGoalDefenseDepthChart = new DepthChart(),
            KickoffOffenseDepthChart = new DepthChart(),
            KickoffDefenseDepthChart = new DepthChart(),
            PuntOffenseDepthChart = new DepthChart(),
            PuntDefenseDepthChart = new DepthChart()
        };
    }

    #endregion
}

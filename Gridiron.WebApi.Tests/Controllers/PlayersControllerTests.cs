using DataAccessLayer.Repositories;
using DomainObjects;
using FluentAssertions;
using Gridiron.WebApi.Controllers;
using Gridiron.WebApi.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Gridiron.WebApi.Tests.Controllers;

/// <summary>
/// Unit tests for PlayersController
/// Tests the controller logic WITHOUT touching the database (mocked repositories)
/// </summary>
public class PlayersControllerTests
{
    private readonly Mock<IPlayerRepository> _mockPlayerRepository;
    private readonly Mock<ILogger<PlayersController>> _mockLogger;
    private readonly PlayersController _controller;

    public PlayersControllerTests()
    {
        _mockPlayerRepository = new Mock<IPlayerRepository>();
        _mockLogger = new Mock<ILogger<PlayersController>>();
        _controller = new PlayersController(_mockPlayerRepository.Object, _mockLogger.Object);
    }

    #region GetPlayers Tests

    [Fact]
    public async Task GetPlayers_WithNoTeamIdFilter_ReturnsAllPlayers()
    {
        // Arrange
        var players = new List<Player>
        {
            CreateTestPlayer(1, "Matt", "Ryan", Positions.QB),
            CreateTestPlayer(2, "Julio", "Jones", Positions.WR)
        };
        _mockPlayerRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(players);

        // Act
        var result = await _controller.GetPlayers(null);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var playerDtos = okResult!.Value as IEnumerable<PlayerDto>;
        playerDtos.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetPlayers_WithTeamIdFilter_ReturnsPlayersForTeam()
    {
        // Arrange
        var players = new List<Player>
        {
            CreateTestPlayer(1, "Matt", "Ryan", Positions.QB, teamId: 1),
            CreateTestPlayer(2, "Julio", "Jones", Positions.WR, teamId: 1)
        };
        _mockPlayerRepository.Setup(repo => repo.GetByTeamIdAsync(1)).ReturnsAsync(players);

        // Act
        var result = await _controller.GetPlayers(teamId: 1);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var playerDtos = okResult!.Value as IEnumerable<PlayerDto>;
        playerDtos.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetPlayers_WithTeamIdFilter_CallsCorrectRepositoryMethod()
    {
        // Arrange
        _mockPlayerRepository.Setup(repo => repo.GetByTeamIdAsync(1)).ReturnsAsync(new List<Player>());

        // Act
        await _controller.GetPlayers(teamId: 1);

        // Assert
        _mockPlayerRepository.Verify(repo => repo.GetByTeamIdAsync(1), Times.Once);
        _mockPlayerRepository.Verify(repo => repo.GetAllAsync(), Times.Never);
    }

    [Fact]
    public async Task GetPlayers_WithoutTeamIdFilter_CallsGetAllAsync()
    {
        // Arrange
        _mockPlayerRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Player>());

        // Act
        await _controller.GetPlayers(null);

        // Assert
        _mockPlayerRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
        _mockPlayerRepository.Verify(repo => repo.GetByTeamIdAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetPlayers_MapsToDtoCorrectly()
    {
        // Arrange
        var player = CreateTestPlayer(1, "Matt", "Ryan", Positions.QB);
        player.Speed = 75;
        player.Strength = 80;
        player.Passing = 95;
        player.Health = 100;

        _mockPlayerRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Player> { player });

        // Act
        var result = await _controller.GetPlayers(null);

        // Assert
        var okResult = result.Result as OkObjectResult;
        var playerDtos = okResult!.Value as IEnumerable<PlayerDto>;
        var playerDto = playerDtos!.First();

        playerDto.Id.Should().Be(1);
        playerDto.FirstName.Should().Be("Matt");
        playerDto.LastName.Should().Be("Ryan");
        playerDto.FullName.Should().Be("Matt Ryan");
        playerDto.Position.Should().Be("QB");
        playerDto.Speed.Should().Be(75);
        playerDto.Strength.Should().Be(80);
        playerDto.Passing.Should().Be(95);
        playerDto.Health.Should().Be(100);
    }

    #endregion

    #region GetPlayer Tests

    [Fact]
    public async Task GetPlayer_WhenPlayerExists_ReturnsOkWithPlayer()
    {
        // Arrange
        var player = CreateTestPlayer(1, "Matt", "Ryan", Positions.QB);
        _mockPlayerRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(player);

        // Act
        var result = await _controller.GetPlayer(1);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var playerDetailDto = okResult!.Value as PlayerDetailDto;
        playerDetailDto!.Id.Should().Be(1);
        playerDetailDto.FirstName.Should().Be("Matt");
        playerDetailDto.LastName.Should().Be("Ryan");
    }

    [Fact]
    public async Task GetPlayer_WhenPlayerNotFound_Returns404()
    {
        // Arrange
        _mockPlayerRepository.Setup(repo => repo.GetByIdAsync(999)).ReturnsAsync((Player?)null);

        // Act
        var result = await _controller.GetPlayer(999);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetPlayer_CallsRepositoryWithCorrectId()
    {
        // Arrange
        var player = CreateTestPlayer(42, "Test", "Player", Positions.QB);
        _mockPlayerRepository.Setup(repo => repo.GetByIdAsync(42)).ReturnsAsync(player);

        // Act
        await _controller.GetPlayer(42);

        // Assert
        _mockPlayerRepository.Verify(repo => repo.GetByIdAsync(42), Times.Once);
    }

    [Fact]
    public async Task GetPlayer_IncludesGameStats()
    {
        // Arrange
        var player = CreateTestPlayer(1, "Matt", "Ryan", Positions.QB);
        player.Stats = new Dictionary<StatTypes.PlayerStatType, int>
        {
            { StatTypes.PlayerStatType.PassingYards, 300 },
            { StatTypes.PlayerStatType.PassingTouchdowns, 2 }
        };

        _mockPlayerRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(player);

        // Act
        var result = await _controller.GetPlayer(1);

        // Assert
        var okResult = result.Result as OkObjectResult;
        var playerDetailDto = okResult!.Value as PlayerDetailDto;
        playerDetailDto!.GameStats.Should().ContainKey("PassingYards");
        playerDetailDto.GameStats["PassingYards"].Should().Be(300);
        playerDetailDto.GameStats["PassingTouchdowns"].Should().Be(2);
    }

    [Fact]
    public async Task GetPlayer_WhenStatsNull_ReturnsEmptyDictionaries()
    {
        // Arrange
        var player = CreateTestPlayer(1, "Matt", "Ryan", Positions.QB);
        player.Stats = null;
        player.SeasonStats = null;
        player.CareerStats = null;

        _mockPlayerRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(player);

        // Act
        var result = await _controller.GetPlayer(1);

        // Assert
        var okResult = result.Result as OkObjectResult;
        var playerDetailDto = okResult!.Value as PlayerDetailDto;
        playerDetailDto!.GameStats.Should().BeEmpty();
        playerDetailDto.SeasonStats.Should().BeEmpty();
        playerDetailDto.CareerStats.Should().BeEmpty();
    }

    #endregion

    #region Helper Methods

    private Player CreateTestPlayer(int id, string firstName, string lastName, Positions position, int? teamId = null)
    {
        return new Player
        {
            Id = id,
            FirstName = firstName,
            LastName = lastName,
            Position = position,
            Number = id,
            Height = "6-2",
            Weight = 200,
            Age = 25,
            Exp = 3,
            College = "Test University",
            TeamId = teamId,
            Speed = 70,
            Strength = 70,
            Agility = 70,
            Awareness = 70,
            Morale = 80,
            Discipline = 85,
            Health = 100
        };
    }

    #endregion
}

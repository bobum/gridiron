using DataAccessLayer.Repositories;
using DomainObjects;
using FluentAssertions;
using GameManagement.Services;
using Gridiron.WebApi.Controllers;
using Gridiron.WebApi.DTOs;
using Gridiron.WebApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;

namespace Gridiron.WebApi.Tests.Controllers;

/// <summary>
/// Unit tests for PlayersController
/// Tests the controller logic WITHOUT touching the database (mocked repositories)
/// </summary>
public class PlayersControllerTests
{
    private readonly Mock<IPlayerRepository> _mockPlayerRepository;
    private readonly Mock<IPlayerGeneratorService> _mockPlayerGeneratorService;
    private readonly Mock<IGridironAuthorizationService> _mockAuthorizationService;
    private readonly Mock<ILogger<PlayersController>> _mockLogger;
    private readonly PlayersController _controller;

    public PlayersControllerTests()
    {
        _mockPlayerRepository = new Mock<IPlayerRepository>();
        _mockPlayerGeneratorService = new Mock<IPlayerGeneratorService>();
        _mockAuthorizationService = new Mock<IGridironAuthorizationService>();
        _mockLogger = new Mock<ILogger<PlayersController>>();
        _controller = new PlayersController(
            _mockPlayerRepository.Object,
            _mockPlayerGeneratorService.Object,
            _mockAuthorizationService.Object,
            _mockLogger.Object);

        // Set up HttpContext with authenticated user claims
        SetupHttpContextWithClaims("test-oid-123", "test@example.com", "Test User");
    }

    private void SetupHttpContextWithClaims(string oid, string email, string displayName)
    {
        var claims = new List<Claim>
        {
            new Claim("oid", oid),
            new Claim("email", email),
            new Claim("name", displayName)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = claimsPrincipal
        };

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    private void SetupAuthorizationMocks(bool canAccessTeam = true, bool isGlobalAdmin = false)
    {
        var testUser = new User { Id = 1, AzureAdObjectId = "test-oid-123", Email = "test@example.com", DisplayName = "Test User" };

        _mockAuthorizationService
            .Setup(s => s.GetOrCreateUserFromClaimsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(testUser);

        _mockAuthorizationService
            .Setup(s => s.CanAccessTeamAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(canAccessTeam);

        _mockAuthorizationService
            .Setup(s => s.IsGlobalAdminAsync(It.IsAny<string>()))
            .ReturnsAsync(isGlobalAdmin);

        _mockAuthorizationService
            .Setup(s => s.GetAccessibleTeamIdsAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<int> { 1, 2, 3 });
    }

    #region GetPlayers Tests

    [Fact]
    public async Task GetPlayers_WithNoTeamIdFilter_ReturnsAllPlayers()
    {
        // Arrange
        SetupAuthorizationMocks(isGlobalAdmin: true);
        var players = new List<Player>
        {
            CreateTestPlayer(1, "Matt", "Ryan", Positions.QB, teamId: 1),
            CreateTestPlayer(2, "Julio", "Jones", Positions.WR, teamId: 1)
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
        SetupAuthorizationMocks(canAccessTeam: true);
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
        SetupAuthorizationMocks(canAccessTeam: true);
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
        SetupAuthorizationMocks(isGlobalAdmin: true);
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
        SetupAuthorizationMocks(isGlobalAdmin: true);
        var player = CreateTestPlayer(1, "Matt", "Ryan", Positions.QB, teamId: 1);
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
        SetupAuthorizationMocks(canAccessTeam: true);
        var player = CreateTestPlayer(1, "Matt", "Ryan", Positions.QB, teamId: 1);
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
        SetupAuthorizationMocks();
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
        SetupAuthorizationMocks(canAccessTeam: true);
        var player = CreateTestPlayer(42, "Test", "Player", Positions.QB, teamId: 1);
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
        SetupAuthorizationMocks(canAccessTeam: true);
        var player = CreateTestPlayer(1, "Matt", "Ryan", Positions.QB, teamId: 1);
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
        SetupAuthorizationMocks(canAccessTeam: true);
        var player = CreateTestPlayer(1, "Matt", "Ryan", Positions.QB, teamId: 1);
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

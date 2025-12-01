using DomainObjects;
using FluentAssertions;
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
/// Unit tests for GamesController
/// Tests the controller logic WITHOUT touching the database (mocked services)
/// Note: TotalPlays is not populated from Game entity (plays are stored in PlayByPlay.PlaysJson)
/// </summary>
public class GamesControllerTests
{
    private readonly Mock<IGameSimulationService> _mockSimulationService;
    private readonly Mock<IGridironAuthorizationService> _mockAuthorizationService;
    private readonly Mock<ILogger<GamesController>> _mockLogger;
    private readonly GamesController _controller;

    public GamesControllerTests()
    {
        _mockSimulationService = new Mock<IGameSimulationService>();
        _mockAuthorizationService = new Mock<IGridironAuthorizationService>();
        _mockLogger = new Mock<ILogger<GamesController>>();
        _controller = new GamesController(
            _mockSimulationService.Object,
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
            .ReturnsAsync(new List<int> { 1, 2, 3, 4, 5 });
    }

    #region SimulateGame Tests

    [Fact]
    public async Task SimulateGame_WhenSuccessful_ReturnsOkWithGameDto()
    {
        // Arrange
        SetupAuthorizationMocks(canAccessTeam: true);
        var request = new SimulateGameRequest
        {
            HomeTeamId = 1,
            AwayTeamId = 2,
            RandomSeed = 12345
        };

        var simulatedGame = CreateTestGame(1, 2, 21, 14, seed: 12345);
        _mockSimulationService
            .Setup(s => s.SimulateGameAsync(1, 2, 12345))
            .ReturnsAsync(simulatedGame);

        // Act
        var result = await _controller.SimulateGame(request);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var gameDto = okResult!.Value as GameDto;

        gameDto.Should().NotBeNull();
        gameDto!.Id.Should().BeGreaterThan(0);
        gameDto.HomeTeamId.Should().Be(1);
        gameDto.AwayTeamId.Should().Be(2);
        gameDto.HomeScore.Should().Be(21);
        gameDto.AwayScore.Should().Be(14);
        gameDto.RandomSeed.Should().Be(12345);
        gameDto.IsComplete.Should().BeTrue();
    }

    [Fact]
    public async Task SimulateGame_WhenSuccessful_CallsSimulationServiceWithCorrectParameters()
    {
        // Arrange
        SetupAuthorizationMocks(canAccessTeam: true);
        var request = new SimulateGameRequest
        {
            HomeTeamId = 5,
            AwayTeamId = 10,
            RandomSeed = 99999
        };

        var simulatedGame = CreateTestGame(5, 10, 17, 24, seed: 99999);
        _mockSimulationService
            .Setup(s => s.SimulateGameAsync(5, 10, 99999))
            .ReturnsAsync(simulatedGame);

        // Act
        await _controller.SimulateGame(request);

        // Assert
        _mockSimulationService.Verify(
            s => s.SimulateGameAsync(5, 10, 99999),
            Times.Once
        );
    }

    [Fact]
    public async Task SimulateGame_WithoutSeed_CallsSimulationServiceWithNullSeed()
    {
        // Arrange
        SetupAuthorizationMocks(canAccessTeam: true);
        var request = new SimulateGameRequest
        {
            HomeTeamId = 1,
            AwayTeamId = 2
            // No seed specified
        };

        var simulatedGame = CreateTestGame(1, 2, 14, 7);
        _mockSimulationService
            .Setup(s => s.SimulateGameAsync(1, 2, null))
            .ReturnsAsync(simulatedGame);

        // Act
        await _controller.SimulateGame(request);

        // Assert
        _mockSimulationService.Verify(
            s => s.SimulateGameAsync(1, 2, null),
            Times.Once
        );
    }

    [Fact]
    public async Task SimulateGame_WhenTeamNotFound_ReturnsBadRequest()
    {
        // Arrange
        SetupAuthorizationMocks(canAccessTeam: true);
        var request = new SimulateGameRequest
        {
            HomeTeamId = 999,
            AwayTeamId = 2
        };

        _mockSimulationService
            .Setup(s => s.SimulateGameAsync(999, 2, null))
            .ThrowsAsync(new ArgumentException("Home team with ID 999 not found", nameof(request.HomeTeamId)));

        // Act
        var result = await _controller.SimulateGame(request);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task SimulateGame_WhenServiceThrowsException_Returns500()
    {
        // Arrange
        SetupAuthorizationMocks(canAccessTeam: true);
        var request = new SimulateGameRequest
        {
            HomeTeamId = 1,
            AwayTeamId = 2
        };

        _mockSimulationService
            .Setup(s => s.SimulateGameAsync(1, 2, null))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _controller.SimulateGame(request);

        // Assert
        result.Result.Should().BeOfType<ObjectResult>();
        var objectResult = result.Result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task SimulateGame_MapsDtoFieldsCorrectly()
    {
        // Arrange
        SetupAuthorizationMocks(canAccessTeam: true);
        var request = new SimulateGameRequest
        {
            HomeTeamId = 1,
            AwayTeamId = 2
        };

        var game = CreateTestGame(1, 2, 31, 27);
        game.Id = 42;

        _mockSimulationService
            .Setup(s => s.SimulateGameAsync(1, 2, null))
            .ReturnsAsync(game);

        // Act
        var result = await _controller.SimulateGame(request);

        // Assert
        var okResult = result.Result as OkObjectResult;
        var gameDto = okResult!.Value as GameDto;

        gameDto!.Id.Should().Be(42);
        gameDto.HomeTeamId.Should().Be(1);
        gameDto.AwayTeamId.Should().Be(2);
        gameDto.HomeTeamName.Should().Be("Atlanta Falcons");
        gameDto.AwayTeamName.Should().Be("Philadelphia Eagles");
        gameDto.HomeScore.Should().Be(31);
        gameDto.AwayScore.Should().Be(27);
        gameDto.IsComplete.Should().BeTrue();
    }

    #endregion

    #region GetGames Tests

    [Fact]
    public async Task GetGames_WhenGamesExist_ReturnsOkWithGames()
    {
        // Arrange
        SetupAuthorizationMocks(isGlobalAdmin: true);
        var games = new List<Game>
        {
            CreateTestGame(1, 2, 21, 14),
            CreateTestGame(3, 4, 28, 24)
        };

        _mockSimulationService.Setup(s => s.GetGamesAsync()).ReturnsAsync(games);

        // Act
        var result = await _controller.GetGames();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var gameDtos = okResult!.Value as IEnumerable<GameDto>;

        gameDtos.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetGames_WhenNoGames_ReturnsEmptyList()
    {
        // Arrange
        SetupAuthorizationMocks(isGlobalAdmin: true);
        _mockSimulationService.Setup(s => s.GetGamesAsync()).ReturnsAsync(new List<Game>());

        // Act
        var result = await _controller.GetGames();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var gameDtos = okResult!.Value as IEnumerable<GameDto>;

        gameDtos.Should().BeEmpty();
    }

    #endregion

    #region GetGame Tests

    [Fact]
    public async Task GetGame_WhenGameExists_ReturnsOkWithGame()
    {
        // Arrange
        SetupAuthorizationMocks(canAccessTeam: true);
        var game = CreateTestGame(1, 2, 17, 10);
        game.Id = 5;

        _mockSimulationService.Setup(s => s.GetGameAsync(5)).ReturnsAsync(game);

        // Act
        var result = await _controller.GetGame(5);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var gameDto = okResult!.Value as GameDto;

        gameDto!.Id.Should().Be(5);
    }

    [Fact]
    public async Task GetGame_WhenGameNotFound_Returns404()
    {
        // Arrange
        SetupAuthorizationMocks(canAccessTeam: true);
        _mockSimulationService.Setup(s => s.GetGameAsync(999)).ReturnsAsync((Game?)null);

        // Act
        var result = await _controller.GetGame(999);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Creates a test game with basic data for testing
    /// Note: Plays are stored in PlayByPlay.PlaysJson, not on the Game entity
    /// </summary>
    private Game CreateTestGame(int homeTeamId, int awayTeamId, int homeScore, int awayScore, int? seed = null)
    {
        var homeTeam = new Team
        {
            Id = homeTeamId,
            Name = "Falcons",
            City = "Atlanta",
            Players = new List<Player>()
        };

        var awayTeam = new Team
        {
            Id = awayTeamId,
            Name = "Eagles",
            City = "Philadelphia",
            Players = new List<Player>()
        };

        var game = new Game
        {
            Id = 1,
            HomeTeamId = homeTeamId,
            AwayTeamId = awayTeamId,
            HomeTeam = homeTeam,
            AwayTeam = awayTeam,
            HomeScore = homeScore,
            AwayScore = awayScore,
            RandomSeed = seed
        };

        return game;
    }

    #endregion
}

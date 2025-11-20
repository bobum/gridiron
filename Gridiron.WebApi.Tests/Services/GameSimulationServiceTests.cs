using DataAccessLayer.Repositories;
using DomainObjects;
using FluentAssertions;
using Gridiron.WebApi.Services;
using Microsoft.Extensions.Logging;
using Moq;
using StateLibrary;
using UnitTestProject1.Helpers;
using Xunit;

namespace Gridiron.WebApi.Tests.Services;

/// <summary>
/// Unit tests for GameSimulationService
/// Tests service logic WITHOUT touching the database (mocked repositories)
/// Note: These tests will run actual game simulations (GameFlow.Execute)
/// </summary>
public class GameSimulationServiceTests
{
    private readonly Mock<ITeamRepository> _mockTeamRepository;
    private readonly Mock<IGameRepository> _mockGameRepository;
    private readonly Mock<ILogger<GameFlow>> _mockGameLogger;
    private readonly Mock<ILogger<GameSimulationService>> _mockLogger;
    private readonly GameSimulationService _service;

    public GameSimulationServiceTests()
    {
        _mockTeamRepository = new Mock<ITeamRepository>();
        _mockGameRepository = new Mock<IGameRepository>();
        _mockGameLogger = new Mock<ILogger<GameFlow>>();
        _mockLogger = new Mock<ILogger<GameSimulationService>>();

        _service = new GameSimulationService(
            _mockTeamRepository.Object,
            _mockGameRepository.Object,
            _mockGameLogger.Object,
            _mockLogger.Object
        );
    }

    #region SimulateGameAsync Tests

    [Fact]
    public async Task SimulateGame_WhenBothTeamsExist_LoadsTeamsWithPlayers()
    {
        // Arrange
        var homeTeam = TestTeams.LoadAtlantaFalcons();
        var awayTeam = TestTeams.LoadPhiladelphiaEagles();
        homeTeam.Id = 1;
        awayTeam.Id = 2;

        _mockTeamRepository.Setup(repo => repo.GetByIdWithPlayersAsync(1)).ReturnsAsync(homeTeam);
        _mockTeamRepository.Setup(repo => repo.GetByIdWithPlayersAsync(2)).ReturnsAsync(awayTeam);
        _mockGameRepository.Setup(repo => repo.AddAsync(It.IsAny<Game>())).ReturnsAsync((Game g) => g);

        // Act
        var result = await _service.SimulateGameAsync(1, 2, 12345);

        // Assert
        _mockTeamRepository.Verify(repo => repo.GetByIdWithPlayersAsync(1), Times.Once);
        _mockTeamRepository.Verify(repo => repo.GetByIdWithPlayersAsync(2), Times.Once);
    }

    [Fact]
    public async Task SimulateGame_WhenHomeTeamNotFound_ThrowsArgumentException()
    {
        // Arrange
        _mockTeamRepository.Setup(repo => repo.GetByIdWithPlayersAsync(999)).ReturnsAsync((Team?)null);

        // Act
        Func<Task> act = async () => await _service.SimulateGameAsync(999, 2);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Home team*not found*")
            .WithParameterName("homeTeamId");
    }

    [Fact]
    public async Task SimulateGame_WhenAwayTeamNotFound_ThrowsArgumentException()
    {
        // Arrange
        var homeTeam = TestTeams.LoadAtlantaFalcons();
        homeTeam.Id = 1;
        _mockTeamRepository.Setup(repo => repo.GetByIdWithPlayersAsync(1)).ReturnsAsync(homeTeam);
        _mockTeamRepository.Setup(repo => repo.GetByIdWithPlayersAsync(999)).ReturnsAsync((Team?)null);

        // Act
        Func<Task> act = async () => await _service.SimulateGameAsync(1, 999);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Away team*not found*")
            .WithParameterName("awayTeamId");
    }

    [Fact]
    public async Task SimulateGame_WhenTeamsLoaded_RunsSimulation()
    {
        // Arrange
        var homeTeam = TestTeams.LoadAtlantaFalcons();
        var awayTeam = TestTeams.LoadPhiladelphiaEagles();
        homeTeam.Id = 1;
        awayTeam.Id = 2;

        _mockTeamRepository.Setup(repo => repo.GetByIdWithPlayersAsync(1)).ReturnsAsync(homeTeam);
        _mockTeamRepository.Setup(repo => repo.GetByIdWithPlayersAsync(2)).ReturnsAsync(awayTeam);
        _mockGameRepository.Setup(repo => repo.AddAsync(It.IsAny<Game>())).ReturnsAsync((Game g) => g);

        // Act
        var result = await _service.SimulateGameAsync(1, 2, 12345);

        // Assert - game should have been played (scores will be set)
        result.Should().NotBeNull();
        result.HomeScore.Should().BeGreaterOrEqualTo(0);
        result.AwayScore.Should().BeGreaterOrEqualTo(0);
        result.Plays.Should().NotBeEmpty(); // Game creates plays
    }

    [Fact]
    public async Task SimulateGame_AfterSimulation_SavesGameToRepository()
    {
        // Arrange
        var homeTeam = TestTeams.LoadAtlantaFalcons();
        var awayTeam = TestTeams.LoadPhiladelphiaEagles();
        homeTeam.Id = 1;
        awayTeam.Id = 2;

        _mockTeamRepository.Setup(repo => repo.GetByIdWithPlayersAsync(1)).ReturnsAsync(homeTeam);
        _mockTeamRepository.Setup(repo => repo.GetByIdWithPlayersAsync(2)).ReturnsAsync(awayTeam);
        _mockGameRepository.Setup(repo => repo.AddAsync(It.IsAny<Game>())).ReturnsAsync((Game g) => g);

        // Act
        await _service.SimulateGameAsync(1, 2, 12345);

        // Assert
        _mockGameRepository.Verify(repo => repo.AddAsync(It.IsAny<Game>()), Times.Once);
    }

    [Fact]
    public async Task SimulateGame_WithSeed_UsesSeedForSimulation()
    {
        // Arrange
        var homeTeam = TestTeams.LoadAtlantaFalcons();
        var awayTeam = TestTeams.LoadPhiladelphiaEagles();
        homeTeam.Id = 1;
        awayTeam.Id = 2;

        _mockTeamRepository.Setup(repo => repo.GetByIdWithPlayersAsync(1)).ReturnsAsync(homeTeam);
        _mockTeamRepository.Setup(repo => repo.GetByIdWithPlayersAsync(2)).ReturnsAsync(awayTeam);
        _mockGameRepository.Setup(repo => repo.AddAsync(It.IsAny<Game>())).ReturnsAsync((Game g) => g);

        // Act
        var result = await _service.SimulateGameAsync(1, 2, randomSeed: 12345);

        // Assert
        result.RandomSeed.Should().Be(12345);
    }

    [Fact]
    public async Task SimulateGame_WithSameSeed_ProducesDeterministicResults()
    {
        // Arrange
        var homeTeam1 = TestTeams.LoadAtlantaFalcons();
        var awayTeam1 = TestTeams.LoadPhiladelphiaEagles();
        homeTeam1.Id = 1;
        awayTeam1.Id = 2;
        var homeTeam2 = TestTeams.LoadAtlantaFalcons();
        var awayTeam2 = TestTeams.LoadPhiladelphiaEagles();
        homeTeam2.Id = 1;
        awayTeam2.Id = 2;

        // First simulation
        _mockTeamRepository.Setup(repo => repo.GetByIdWithPlayersAsync(1)).ReturnsAsync(homeTeam1);
        _mockTeamRepository.Setup(repo => repo.GetByIdWithPlayersAsync(2)).ReturnsAsync(awayTeam1);
        _mockGameRepository.Setup(repo => repo.AddAsync(It.IsAny<Game>())).ReturnsAsync((Game g) => g);

        var result1 = await _service.SimulateGameAsync(1, 2, randomSeed: 99999);

        // Second simulation with same seed
        _mockTeamRepository.Setup(repo => repo.GetByIdWithPlayersAsync(1)).ReturnsAsync(homeTeam2);
        _mockTeamRepository.Setup(repo => repo.GetByIdWithPlayersAsync(2)).ReturnsAsync(awayTeam2);

        var result2 = await _service.SimulateGameAsync(1, 2, randomSeed: 99999);

        // Assert - same seed should produce same results
        result1.HomeScore.Should().Be(result2.HomeScore);
        result1.AwayScore.Should().Be(result2.AwayScore);
    }

    [Fact]
    public async Task SimulateGame_ReturnsGameWithCorrectTeamIds()
    {
        // Arrange
        var homeTeam = TestTeams.LoadAtlantaFalcons();
        var awayTeam = TestTeams.LoadPhiladelphiaEagles();
        homeTeam.Id = 10;
        awayTeam.Id = 20;

        _mockTeamRepository.Setup(repo => repo.GetByIdWithPlayersAsync(10)).ReturnsAsync(homeTeam);
        _mockTeamRepository.Setup(repo => repo.GetByIdWithPlayersAsync(20)).ReturnsAsync(awayTeam);
        _mockGameRepository.Setup(repo => repo.AddAsync(It.IsAny<Game>())).ReturnsAsync((Game g) => g);

        // Act
        var result = await _service.SimulateGameAsync(10, 20, 12345);

        // Assert
        result.HomeTeamId.Should().Be(10);
        result.AwayTeamId.Should().Be(20);
        result.HomeTeam.Should().NotBeNull();
        result.AwayTeam.Should().NotBeNull();
    }

    #endregion

    #region GetGameAsync Tests

    [Fact]
    public async Task GetGame_WhenGameExists_ReturnsGameWithTeams()
    {
        // Arrange
        var game = CreateTestGame(1, 10, 20);
        _mockGameRepository.Setup(repo => repo.GetByIdWithTeamsAsync(1)).ReturnsAsync(game);

        // Act
        var result = await _service.GetGameAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.HomeTeam.Should().NotBeNull();
        result.AwayTeam.Should().NotBeNull();
    }

    [Fact]
    public async Task GetGame_WhenGameNotFound_ReturnsNull()
    {
        // Arrange
        _mockGameRepository.Setup(repo => repo.GetByIdWithTeamsAsync(999)).ReturnsAsync((Game?)null);

        // Act
        var result = await _service.GetGameAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetGame_CallsRepositoryWithCorrectId()
    {
        // Arrange
        _mockGameRepository.Setup(repo => repo.GetByIdWithTeamsAsync(42)).ReturnsAsync((Game?)null);

        // Act
        await _service.GetGameAsync(42);

        // Assert
        _mockGameRepository.Verify(repo => repo.GetByIdWithTeamsAsync(42), Times.Once);
    }

    #endregion

    #region GetGamesAsync Tests

    [Fact]
    public async Task GetGames_WhenGamesExist_ReturnsAllGames()
    {
        // Arrange
        var games = new List<Game>
        {
            CreateTestGame(1, 1, 2),
            CreateTestGame(2, 3, 4)
        };
        _mockGameRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(games);

        // Act
        var result = await _service.GetGamesAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetGames_WhenNoGames_ReturnsEmptyList()
    {
        // Arrange
        _mockGameRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Game>());

        // Act
        var result = await _service.GetGamesAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region Helper Methods

    private Game CreateTestGame(int id, int homeTeamId, int awayTeamId)
    {
        return new Game
        {
            Id = id,
            HomeTeamId = homeTeamId,
            AwayTeamId = awayTeamId,
            HomeTeam = new Team { Id = homeTeamId, Name = $"Team{homeTeamId}", City = "City" },
            AwayTeam = new Team { Id = awayTeamId, Name = $"Team{awayTeamId}", City = "City" },
            HomeScore = 21,
            AwayScore = 14
        };
    }

    #endregion
}

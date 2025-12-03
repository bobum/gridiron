using DataAccessLayer.Repositories;
using DomainObjects;
using FluentAssertions;
using GameManagement.Services;
using Gridiron.TestHelpers.Helpers;
using Gridiron.WebApi.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Gridiron.WebApi.Tests.Services;

public class GameSimulationServiceTests
{
    private readonly Mock<ITeamRepository> _mockTeamRepository;
    private readonly Mock<IGameRepository> _mockGameRepository;
    private readonly Mock<IPlayByPlayRepository> _mockPlayByPlayRepository;
    private readonly Mock<IEngineSimulationService> _mockEngineSimulationService;
    private readonly Mock<ILogger<GameSimulationService>> _mockLogger;
    private readonly GameSimulationService _service;

    public GameSimulationServiceTests()
    {
        _mockTeamRepository = new Mock<ITeamRepository>();
        _mockGameRepository = new Mock<IGameRepository>();
        _mockPlayByPlayRepository = new Mock<IPlayByPlayRepository>();
        _mockEngineSimulationService = new Mock<IEngineSimulationService>();
        _mockLogger = new Mock<ILogger<GameSimulationService>>();

        _service = new GameSimulationService(
            _mockTeamRepository.Object,
            _mockGameRepository.Object,
            _mockPlayByPlayRepository.Object,
            _mockEngineSimulationService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task SimulateGame_WhenHomeTeamNotFound_ThrowsArgumentException()
    {
        _mockTeamRepository.Setup(repo => repo.GetByIdWithPlayersAsync(999)).ReturnsAsync((Team?)null);
        Func<Task> act = async () => await _service.SimulateGameAsync(999, 2);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Home team*not found*")
            .WithParameterName("homeTeamId");
    }

    [Fact]
    public async Task SimulateGame_WhenAwayTeamNotFound_ThrowsArgumentException()
    {
        var homeTeam = TestTeams.LoadAtlantaFalcons();
        homeTeam.Id = 1;
        _mockTeamRepository.Setup(repo => repo.GetByIdWithPlayersAsync(1)).ReturnsAsync(homeTeam);
        _mockTeamRepository.Setup(repo => repo.GetByIdWithPlayersAsync(999)).ReturnsAsync((Team?)null);
        Func<Task> act = async () => await _service.SimulateGameAsync(1, 999);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Away team*not found*")
            .WithParameterName("awayTeamId");
    }

    [Fact]
    public async Task GetGame_WhenGameNotFound_ReturnsNull()
    {
        _mockGameRepository.Setup(repo => repo.GetByIdWithTeamsAsync(999)).ReturnsAsync((Game?)null);
        var result = await _service.GetGameAsync(999);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetGames_WhenNoGames_ReturnsEmptyList()
    {
        _mockGameRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Game>());
        var result = await _service.GetGamesAsync();
        result.Should().BeEmpty();
    }
}

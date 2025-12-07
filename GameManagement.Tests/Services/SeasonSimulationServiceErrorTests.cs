using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DataAccessLayer;
using DataAccessLayer.Repositories;
using DomainObjects;
using GameManagement.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GameManagement.Tests.Services;

public class SeasonSimulationServiceErrorTests
{
    private Mock<ISeasonRepository> _mockSeasonRepository;
    private Mock<IGameRepository> _mockGameRepository;
    private Mock<ITeamRepository> _mockTeamRepository;
    private Mock<IPlayByPlayRepository> _mockPlayByPlayRepository;
    private Mock<IPlayerGameStatRepository> _mockPlayerGameStatRepository;
    private Mock<IEngineSimulationService> _mockEngineSimulationService;
    private Mock<ITransactionManager> _mockTransactionManager;
    private Mock<ILogger<SeasonSimulationService>> _mockLogger;
    private SeasonSimulationService _service;

    public SeasonSimulationServiceErrorTests()
    {
        _mockSeasonRepository = new Mock<ISeasonRepository>();
        _mockGameRepository = new Mock<IGameRepository>();
        _mockTeamRepository = new Mock<ITeamRepository>();
        _mockPlayByPlayRepository = new Mock<IPlayByPlayRepository>();
        _mockPlayerGameStatRepository = new Mock<IPlayerGameStatRepository>();
        _mockEngineSimulationService = new Mock<IEngineSimulationService>();
        _mockTransactionManager = new Mock<ITransactionManager>();
        _mockLogger = new Mock<ILogger<SeasonSimulationService>>();

        var mockTransaction = new Mock<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>();
        _mockTransactionManager.Setup(m => m.BeginTransactionAsync())
            .ReturnsAsync(mockTransaction.Object);

        _service = new SeasonSimulationService(
            _mockSeasonRepository.Object,
            _mockGameRepository.Object,
            _mockTeamRepository.Object,
            _mockPlayByPlayRepository.Object,
            _mockPlayerGameStatRepository.Object,
            _mockEngineSimulationService.Object,
            _mockTransactionManager.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task SimulateCurrentWeekAsync_ShouldReturnError_WhenSeasonNotFound()
    {
        _mockSeasonRepository.Setup(r => r.GetByIdWithWeeksAndGamesAsync(It.IsAny<int>()))
            .ReturnsAsync((Season?)null);

        var result = await _service.SimulateCurrentWeekAsync(1);

        Assert.False(result.Success);
        Assert.Contains("not found", result.Error);
    }

    [Fact]
    public async Task SimulateCurrentWeekAsync_ShouldReturnError_WhenSeasonIsComplete()
    {
        var season = new Season { Id = 1, IsComplete = true };
        _mockSeasonRepository.Setup(r => r.GetByIdWithWeeksAndGamesAsync(1))
            .ReturnsAsync(season);

        var result = await _service.SimulateCurrentWeekAsync(1);

        Assert.False(result.Success);
        Assert.Contains("already complete", result.Error);
    }

    [Fact]
    public async Task SimulateCurrentWeekAsync_ShouldReturnError_WhenWeekNotFound()
    {
        var season = new Season { Id = 1, CurrentWeek = 1, Weeks = new List<SeasonWeek>() };
        _mockSeasonRepository.Setup(r => r.GetByIdWithWeeksAndGamesAsync(1))
            .ReturnsAsync(season);

        var result = await _service.SimulateCurrentWeekAsync(1);

        Assert.False(result.Success);
        Assert.Contains("Week 1 not found", result.Error);
    }

    [Fact]
    public async Task SimulateCurrentWeekAsync_ShouldReturnError_WhenWeekAlreadyCompleted()
    {
        var week = new SeasonWeek { WeekNumber = 1, Status = WeekStatus.Completed };
        var season = new Season { Id = 1, CurrentWeek = 1, Weeks = new List<SeasonWeek> { week } };
        _mockSeasonRepository.Setup(r => r.GetByIdWithWeeksAndGamesAsync(1))
            .ReturnsAsync(season);

        var result = await _service.SimulateCurrentWeekAsync(1);

        Assert.False(result.Success);
        Assert.Contains("already completed", result.Error);
    }

    [Fact]
    public async Task SimulateCurrentWeekAsync_ShouldHandleConcurrencyException()
    {
        var week = new SeasonWeek { WeekNumber = 1, Status = WeekStatus.Scheduled, Games = new List<Game>() };
        var season = new Season { Id = 1, CurrentWeek = 1, Weeks = new List<SeasonWeek> { week } };
        _mockSeasonRepository.Setup(r => r.GetByIdWithWeeksAndGamesAsync(1))
            .ReturnsAsync(season);

        _mockSeasonRepository.Setup(r => r.UpdateAsync(It.IsAny<Season>()))
            .ThrowsAsync(new DbUpdateConcurrencyException());

        var result = await _service.SimulateCurrentWeekAsync(1);

        Assert.False(result.Success);
        Assert.True(result.IsConcurrencyError);
        Assert.Contains("Simulation failed due to concurrent modification", result.Error ?? string.Empty);
    }

    [Fact]
    public async Task SimulateCurrentWeekAsync_ShouldHandleGenericException()
    {
        _mockSeasonRepository.Setup(r => r.GetByIdWithWeeksAndGamesAsync(1))
            .ThrowsAsync(new Exception("Generic error"));

        var result = await _service.SimulateCurrentWeekAsync(1);

        Assert.False(result.Success);
        Assert.Contains("Simulation failed: Generic error", result.Error);
    }

    [Fact]
    public async Task RevertLastWeekAsync_ShouldReturnError_WhenSeasonNotFound()
    {
        _mockSeasonRepository.Setup(r => r.GetByIdWithWeeksAndGamesAsync(It.IsAny<int>()))
            .ReturnsAsync((Season?)null);

        var result = await _service.RevertLastWeekAsync(1);

        Assert.False(result.Success);
        Assert.Contains("not found", result.Error);
    }

    [Fact]
    public async Task RevertLastWeekAsync_ShouldReturnError_WhenNoPreviousWeeks()
    {
        var season = new Season { Id = 1, CurrentWeek = 1, IsComplete = false };
        _mockSeasonRepository.Setup(r => r.GetByIdWithWeeksAndGamesAsync(1))
            .ReturnsAsync(season);

        var result = await _service.RevertLastWeekAsync(1);

        Assert.False(result.Success);
        Assert.Contains("No previous weeks", result.Error);
    }

    [Fact]
    public async Task RevertLastWeekAsync_ShouldReturnError_WhenWeekNotFound()
    {
        var season = new Season { Id = 1, CurrentWeek = 2, IsComplete = false, Weeks = new List<SeasonWeek>() };
        _mockSeasonRepository.Setup(r => r.GetByIdWithWeeksAndGamesAsync(1))
            .ReturnsAsync(season);

        var result = await _service.RevertLastWeekAsync(1);

        Assert.False(result.Success);
        Assert.Contains("Week 1 not found", result.Error);
    }

    [Fact]
    public async Task RevertLastWeekAsync_ShouldReturnError_WhenWeekNotCompleted()
    {
        var week = new SeasonWeek { WeekNumber = 1, Status = WeekStatus.Scheduled };
        var season = new Season { Id = 1, CurrentWeek = 2, IsComplete = false, Weeks = new List<SeasonWeek> { week } };
        _mockSeasonRepository.Setup(r => r.GetByIdWithWeeksAndGamesAsync(1))
            .ReturnsAsync(season);

        var result = await _service.RevertLastWeekAsync(1);

        Assert.False(result.Success);
        Assert.Contains("not completed", result.Error);
    }

    [Fact]
    public async Task RevertLastWeekAsync_ShouldHandleGenericException()
    {
        _mockSeasonRepository.Setup(r => r.GetByIdWithWeeksAndGamesAsync(1))
            .ThrowsAsync(new Exception("Revert error"));

        var result = await _service.RevertLastWeekAsync(1);

        Assert.False(result.Success);
        Assert.Contains("Revert failed: Revert error", result.Error);
    }
}

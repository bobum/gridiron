using DomainObjects;
using GameManagement.Services;
using Microsoft.Extensions.Logging;
using Moq;
using DataAccessLayer.Repositories;
using Xunit;

namespace GameManagement.Tests.Services;

public class SeasonSimulationServiceTests
{
    private Mock<ISeasonRepository> _mockSeasonRepository;
    private Mock<IGameRepository> _mockGameRepository;
    private Mock<IEngineSimulationService> _mockEngineSimulationService;
    private Mock<ILogger<SeasonSimulationService>> _mockLogger;
    private SeasonSimulationService _service;

    public SeasonSimulationServiceTests()
    {
        _mockSeasonRepository = new Mock<ISeasonRepository>();
        _mockGameRepository = new Mock<IGameRepository>();
        _mockEngineSimulationService = new Mock<IEngineSimulationService>();
        _mockLogger = new Mock<ILogger<SeasonSimulationService>>();

        _service = new SeasonSimulationService(
            _mockSeasonRepository.Object,
            _mockGameRepository.Object,
            _mockEngineSimulationService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task SimulateCurrentWeekAsync_ShouldSimulateUnplayedGames()
    {
        // Arrange
        var seasonId = 1;
        var currentWeek = 1;
        var homeTeam = new Team { Id = 1, Name = "Home" };
        var awayTeam = new Team { Id = 2, Name = "Away" };
        
        var game = new Game 
        { 
            Id = 100, 
            HomeTeamId = 1, 
            AwayTeamId = 2,
            IsComplete = false 
        };

        var week = new SeasonWeek 
        { 
            WeekNumber = currentWeek,
            Status = WeekStatus.Scheduled,
            Games = new List<Game> { game }
        };

        var season = new Season 
        { 
            Id = seasonId, 
            CurrentWeek = currentWeek,
            Weeks = new List<SeasonWeek> { week }
        };

        var fullGame = new Game 
        { 
            Id = 100, 
            HomeTeamId = 1, 
            AwayTeamId = 2,
            HomeTeam = homeTeam,
            AwayTeam = awayTeam,
            IsComplete = false 
        };

        var simResult = new EngineSimulationResult 
        { 
            HomeTeam = homeTeam,
            AwayTeam = awayTeam,
            HomeScore = 24,
            AwayScore = 17,
            IsTie = false,
            TotalPlays = 120
        };

        _mockSeasonRepository.Setup(r => r.GetByIdWithWeeksAndGamesAsync(seasonId))
            .ReturnsAsync(season);

        _mockGameRepository.Setup(r => r.GetByIdWithTeamsAsync(game.Id))
            .ReturnsAsync(fullGame);

        _mockEngineSimulationService.Setup(s => s.SimulateGame(It.IsAny<Team>(), It.IsAny<Team>(), null))
            .Returns(simResult);

        // Act
        var result = await _service.SimulateCurrentWeekAsync(seasonId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(1, result.GamesSimulated);
        Assert.Equal(24, result.GameResults[0].HomeScore);
        Assert.Equal(17, result.GameResults[0].AwayScore);
        Assert.Equal(WeekStatus.Completed, week.Status);
        
        _mockGameRepository.Verify(r => r.UpdateAsync(It.Is<Game>(g => g.IsComplete && g.HomeScore == 24)), Times.Once);
        _mockSeasonRepository.Verify(r => r.UpdateAsync(season), Times.AtLeastOnce);
    }

    [Fact]
    public async Task SimulateCurrentWeekAsync_ShouldAdvanceSeasonWeek()
    {
        // Arrange
        var seasonId = 1;
        var currentWeekNum = 1;
        
        var week1 = new SeasonWeek 
        { 
            WeekNumber = 1,
            Status = WeekStatus.Scheduled,
            Games = new List<Game>() // No games to simulate for simplicity
        };

        var week2 = new SeasonWeek 
        { 
            WeekNumber = 2,
            Status = WeekStatus.Scheduled,
            Games = new List<Game>()
        };

        var season = new Season 
        { 
            Id = seasonId, 
            CurrentWeek = currentWeekNum,
            Weeks = new List<SeasonWeek> { week1, week2 }
        };

        _mockSeasonRepository.Setup(r => r.GetByIdWithWeeksAndGamesAsync(seasonId))
            .ReturnsAsync(season);

        // Act
        var result = await _service.SimulateCurrentWeekAsync(seasonId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(2, season.CurrentWeek);
        Assert.False(season.IsComplete);
    }

    [Fact]
    public async Task SimulateCurrentWeekAsync_ShouldCompleteSeason_WhenLastWeekFinished()
    {
        // Arrange
        var seasonId = 1;
        
        var week1 = new SeasonWeek 
        { 
            WeekNumber = 1,
            Status = WeekStatus.Scheduled,
            Games = new List<Game>()
        };

        var season = new Season 
        { 
            Id = seasonId, 
            CurrentWeek = 1,
            Weeks = new List<SeasonWeek> { week1 } // Only one week
        };

        _mockSeasonRepository.Setup(r => r.GetByIdWithWeeksAndGamesAsync(seasonId))
            .ReturnsAsync(season);

        // Act
        var result = await _service.SimulateCurrentWeekAsync(seasonId);

        // Assert
        Assert.True(result.Success);
        Assert.True(result.SeasonCompleted);
        Assert.True(season.IsComplete);
    }
}

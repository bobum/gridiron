using DataAccessLayer;
using DataAccessLayer.Repositories;
using DomainObjects;
using GameManagement.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace GameManagement.Tests.Services;

public class SeasonSimulationServiceTests
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

    public SeasonSimulationServiceTests()
    {
        _mockSeasonRepository = new Mock<ISeasonRepository>();
        _mockGameRepository = new Mock<IGameRepository>();
        _mockTeamRepository = new Mock<ITeamRepository>();
        _mockPlayByPlayRepository = new Mock<IPlayByPlayRepository>();
        _mockPlayerGameStatRepository = new Mock<IPlayerGameStatRepository>();
        _mockEngineSimulationService = new Mock<IEngineSimulationService>();
        _mockTransactionManager = new Mock<ITransactionManager>();
        _mockLogger = new Mock<ILogger<SeasonSimulationService>>();

        // Setup transaction mock
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

        // Add players with stats to test stats logic
        var p1 = new Player { Id = 1, TeamId = 1, Stats = new Dictionary<DomainObjects.StatTypes.PlayerStatType, int> { { DomainObjects.StatTypes.PlayerStatType.PassingYards, 200 } } };
        homeTeam.Players.Add(p1);

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

        _mockGameRepository.Setup(r => r.GetByIdWithTeamsAndPlayersAsync(game.Id))
            .ReturnsAsync(fullGame);

        _mockEngineSimulationService.Setup(s => s.SimulateGame(It.IsAny<Team>(), It.IsAny<Team>(), null, It.IsAny<ILogger>()))
            .Callback<Team, Team, int?, ILogger>((h, a, s, l) =>
            {
                // Simulate engine updating player stats
                var player = h.Players.First();

                // Create new dictionary to force update
                var newStats = new Dictionary<DomainObjects.StatTypes.PlayerStatType, int>(player.Stats);
                newStats[DomainObjects.StatTypes.PlayerStatType.PassingYards] = 250;
                player.Stats = newStats;
            })
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
        _mockTransactionManager.Verify(m => m.BeginTransactionAsync(), Times.Once);

        // Verify PlayerGameStats saved
        _mockPlayerGameStatRepository.Verify(
            r => r.AddRangeAsync(It.Is<IEnumerable<PlayerGameStat>>(stats =>
            stats.Count() == 1 &&
            stats.First().Stats[DomainObjects.StatTypes.PlayerStatType.PassingYards] == 50)), Times.Once);

        // Verify PlayByPlay saved
        _mockPlayByPlayRepository.Verify(r => r.AddAsync(It.IsAny<PlayByPlay>()), Times.Once);
    }

    [Fact]
    public async Task RevertLastWeekAsync_ShouldRevertCompletedWeek()
    {
        // Arrange
        var seasonId = 1;
        var weekNum = 1;

        var game = new Game
        {
            Id = 100,
            HomeTeamId = 1,
            AwayTeamId = 2,
            IsComplete = true,
            HomeScore = 24,
            AwayScore = 17
        };

        var week = new SeasonWeek
        {
            WeekNumber = weekNum,
            Status = WeekStatus.Completed,
            Games = new List<Game> { game }
        };

        var season = new Season
        {
            Id = seasonId,
            CurrentWeek = 2, // Advanced to next week
            Weeks = new List<SeasonWeek> { week }
        };

        var homeTeam = new Team { Id = 1, Wins = 1, Name = "Home" };
        var awayTeam = new Team { Id = 2, Losses = 1, Name = "Away" };
        var fullGame = new Game
        {
            Id = 100,
            HomeTeamId = 1,
            AwayTeamId = 2,
            HomeTeam = homeTeam,
            AwayTeam = awayTeam,
            IsComplete = true,
            HomeScore = 24,
            AwayScore = 17
        };

        _mockSeasonRepository.Setup(r => r.GetByIdWithWeeksAndGamesAsync(seasonId))
            .ReturnsAsync(season);

        _mockGameRepository.Setup(r => r.GetByIdWithTeamsAndPlayersAsync(game.Id))
            .ReturnsAsync(fullGame);

        var playByPlay = new PlayByPlay
        {
            Id = 500,
            GameId = game.Id,
            Game = fullGame,
            PlaysJson = "[]",
            PlayByPlayLog = "Log"
        };
        _mockPlayByPlayRepository.Setup(r => r.GetByGameIdAsync(game.Id))
            .ReturnsAsync(playByPlay);

        // Act
        var result = await _service.RevertLastWeekAsync(seasonId);

        // Assert
        Assert.Null(result.Error);
        Assert.Equal(1, result.WeekNumber);
        Assert.Equal(1, season.CurrentWeek);
        Assert.Equal(WeekStatus.Scheduled, week.Status);
        Assert.False(game.IsComplete);
        Assert.Equal(0, game.HomeScore);
        Assert.Equal(0, game.AwayScore);

        // Verify Team Stats Reverted
        Assert.Equal(0, homeTeam.Wins);
        Assert.Equal(0, awayTeam.Losses);
        _mockTeamRepository.Verify(r => r.UpdateAsync(homeTeam), Times.Once);
        _mockTeamRepository.Verify(r => r.UpdateAsync(awayTeam), Times.Once);

        // Verify PlayerGameStats Deleted
        _mockPlayerGameStatRepository.Verify(r => r.DeleteByGameIdAsync(game.Id), Times.Once);

        // Verify PlayByPlay Deleted
        _mockPlayByPlayRepository.Verify(r => r.DeleteAsync(playByPlay.Id), Times.Once);

        _mockSeasonRepository.Verify(r => r.UpdateAsync(season), Times.Once);
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

    [Fact]
    public async Task SimulateCurrentWeekAsync_ShouldHandleTie()
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
            HomeScore = 20,
            AwayScore = 20,
            IsTie = true,
            TotalPlays = 100
        };

        _mockSeasonRepository.Setup(r => r.GetByIdWithWeeksAndGamesAsync(seasonId))
            .ReturnsAsync(season);

        _mockGameRepository.Setup(r => r.GetByIdWithTeamsAndPlayersAsync(game.Id))
            .ReturnsAsync(fullGame);

        _mockEngineSimulationService.Setup(s => s.SimulateGame(It.IsAny<Team>(), It.IsAny<Team>(), null, It.IsAny<ILogger>()))
            .Returns(simResult);

        // Act
        var result = await _service.SimulateCurrentWeekAsync(seasonId);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(20, result.GameResults[0].HomeScore);
        Assert.Equal(20, result.GameResults[0].AwayScore);
        Assert.True(result.GameResults[0].IsTie);

        Assert.Equal(1, homeTeam.Ties);
        Assert.Equal(1, awayTeam.Ties);

        _mockGameRepository.Verify(r => r.UpdateAsync(It.Is<Game>(g => g.IsComplete && g.HomeScore == 20)), Times.Once);
    }

    [Fact]
    public async Task RevertLastWeekAsync_ShouldRevertTie()
    {
        // Arrange
        var seasonId = 1;
        var weekNum = 1;

        var game = new Game
        {
            Id = 100,
            HomeTeamId = 1,
            AwayTeamId = 2,
            IsComplete = true,
            HomeScore = 20,
            AwayScore = 20
        };

        var week = new SeasonWeek
        {
            WeekNumber = weekNum,
            Status = WeekStatus.Completed,
            Games = new List<Game> { game }
        };

        var season = new Season
        {
            Id = seasonId,
            CurrentWeek = 2,
            Weeks = new List<SeasonWeek> { week }
        };

        var homeTeam = new Team { Id = 1, Ties = 1, Name = "Home" };
        var awayTeam = new Team { Id = 2, Ties = 1, Name = "Away" };
        var fullGame = new Game
        {
            Id = 100,
            HomeTeamId = 1,
            AwayTeamId = 2,
            HomeTeam = homeTeam,
            AwayTeam = awayTeam,
            IsComplete = true,
            HomeScore = 20,
            AwayScore = 20
        };

        _mockSeasonRepository.Setup(r => r.GetByIdWithWeeksAndGamesAsync(seasonId))
            .ReturnsAsync(season);

        _mockGameRepository.Setup(r => r.GetByIdWithTeamsAndPlayersAsync(game.Id))
            .ReturnsAsync(fullGame);

        _mockPlayByPlayRepository.Setup(r => r.GetByGameIdAsync(game.Id))
            .ReturnsAsync((PlayByPlay?)null);

        // Act
        var result = await _service.RevertLastWeekAsync(seasonId);

        // Assert
        Assert.Null(result.Error);

        // Verify Team Stats Reverted
        Assert.Equal(0, homeTeam.Ties);
        Assert.Equal(0, awayTeam.Ties);
        _mockTeamRepository.Verify(r => r.UpdateAsync(homeTeam), Times.Once);
        _mockTeamRepository.Verify(r => r.UpdateAsync(awayTeam), Times.Once);
    }
}

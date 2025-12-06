using DataAccessLayer.Repositories;
using DomainObjects;
using GameManagement.Services;
using Gridiron.WebApi.Controllers;
using Gridiron.WebApi.DTOs;
using Gridiron.WebApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Gridiron.WebApi.Tests.Controllers;

public class SeasonsControllerTests
{
    private readonly Mock<ISeasonRepository> _mockSeasonRepository;
    private readonly Mock<ILeagueRepository> _mockLeagueRepository;
    private readonly Mock<IScheduleGeneratorService> _mockScheduleGeneratorService;
    private readonly Mock<ISeasonSimulationService> _mockSeasonSimulationService;
    private readonly Mock<IGridironAuthorizationService> _mockAuthorizationService;
    private readonly Mock<ILogger<SeasonsController>> _mockLogger;
    private readonly SeasonsController _controller;

    public SeasonsControllerTests()
    {
        _mockSeasonRepository = new Mock<ISeasonRepository>();
        _mockLeagueRepository = new Mock<ILeagueRepository>();
        _mockScheduleGeneratorService = new Mock<IScheduleGeneratorService>();
        _mockSeasonSimulationService = new Mock<ISeasonSimulationService>();
        _mockAuthorizationService = new Mock<IGridironAuthorizationService>();
        _mockLogger = new Mock<ILogger<SeasonsController>>();

        _controller = new SeasonsController(
            _mockSeasonRepository.Object,
            _mockLeagueRepository.Object,
            _mockScheduleGeneratorService.Object,
            _mockSeasonSimulationService.Object,
            _mockAuthorizationService.Object,
            _mockLogger.Object);

        // Setup default context
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    [Fact]
    public async Task GetCurrentWeek_ShouldReturnOk_WhenSeasonExistsAndAuthorized()
    {
        // Arrange
        var seasonId = 1;
        var leagueId = 10;
        var userId = "user-id";
        
        var season = new Season 
        { 
            Id = seasonId, 
            LeagueId = leagueId, 
            CurrentWeek = 1,
            Weeks = new List<SeasonWeek>
            {
                new SeasonWeek 
                { 
                    WeekNumber = 1, 
                    Status = WeekStatus.Scheduled,
                    Games = new List<Game>
                    {
                        new Game { Id = 100, HomeTeam = new Team { Name = "Home" }, AwayTeam = new Team { Name = "Away" } }
                    }
                }
            }
        };

        _mockSeasonRepository.Setup(r => r.GetByIdWithFullDataAsync(seasonId))
            .ReturnsAsync(season);

        // Mock Auth
        var claims = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(new[]
        {
            new System.Security.Claims.Claim("http://schemas.microsoft.com/identity/claims/objectidentifier", userId)
        }));
        _controller.ControllerContext.HttpContext.User = claims;

        _mockAuthorizationService.Setup(a => a.CanAccessLeagueAsync(userId, leagueId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.GetCurrentWeek(seasonId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<CurrentWeekDto>(okResult.Value);
        Assert.Equal(1, dto.WeekNumber);
        Assert.Single(dto.Games);
        Assert.Equal("Home", dto.Games[0].HomeTeamName);
    }
}
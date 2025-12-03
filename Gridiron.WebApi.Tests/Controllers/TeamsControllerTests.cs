using System.Security.Claims;
using DataAccessLayer.Repositories;
using DomainObjects;
using FluentAssertions;
using Gridiron.WebApi.Controllers;
using Gridiron.WebApi.DTOs;
using Gridiron.WebApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Gridiron.WebApi.Tests.Controllers;

/// <summary>
/// Unit tests for TeamsController
/// Tests the controller logic WITHOUT touching the database (mocked repositories).
/// </summary>
public class TeamsControllerTests
{
    private readonly Mock<ITeamRepository> _mockTeamRepository;
    private readonly Mock<IGridironAuthorizationService> _mockAuthorizationService;
    private readonly Mock<ILogger<TeamsController>> _mockLogger;
    private readonly TeamsController _controller;

    public TeamsControllerTests()
    {
        _mockTeamRepository = new Mock<ITeamRepository>();
        _mockAuthorizationService = new Mock<IGridironAuthorizationService>();
        _mockLogger = new Mock<ILogger<TeamsController>>();
        _controller = new TeamsController(
            _mockTeamRepository.Object,
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

    #region GetTeams Tests

    [Fact]
    public async Task GetTeams_WhenTeamsExist_ReturnsOkWithTeams()
    {
        // Arrange
        SetupAuthorizationMocks(isGlobalAdmin: true);
        var teams = new List<Team>
        {
            CreateTestTeam(1, "Falcons", "Atlanta"),
            CreateTestTeam(2, "Eagles", "Philadelphia")
        };
        _mockTeamRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(teams);

        // Act
        var result = await _controller.GetTeams();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var teamDtos = okResult!.Value as IEnumerable<TeamDto>;
        teamDtos.Should().HaveCount(2);
        teamDtos!.First().Name.Should().Be("Falcons");
    }

    [Fact]
    public async Task GetTeams_WhenNoTeams_ReturnsEmptyList()
    {
        // Arrange
        SetupAuthorizationMocks(isGlobalAdmin: true);
        _mockTeamRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Team>());

        // Act
        var result = await _controller.GetTeams();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var teamDtos = okResult!.Value as IEnumerable<TeamDto>;
        teamDtos.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTeams_CallsRepositoryGetAllAsync()
    {
        // Arrange
        SetupAuthorizationMocks(isGlobalAdmin: true);
        _mockTeamRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Team>());

        // Act
        await _controller.GetTeams();

        // Assert
        _mockTeamRepository.Verify(repo => repo.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetTeams_MapsToDtoCorrectly()
    {
        // Arrange
        SetupAuthorizationMocks(isGlobalAdmin: true);
        var team = CreateTestTeam(1, "Falcons", "Atlanta");
        team.Budget = 200000000;
        team.Championships = 5;
        team.Wins = 10;
        team.Losses = 6;
        _mockTeamRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Team> { team });

        // Act
        var result = await _controller.GetTeams();

        // Assert
        var okResult = result.Result as OkObjectResult;
        var teamDtos = okResult!.Value as IEnumerable<TeamDto>;
        var teamDto = teamDtos!.First();

        teamDto.Id.Should().Be(1);
        teamDto.Name.Should().Be("Falcons");
        teamDto.City.Should().Be("Atlanta");
        teamDto.Budget.Should().Be(200000000);
        teamDto.Championships.Should().Be(5);
        teamDto.Wins.Should().Be(10);
        teamDto.Losses.Should().Be(6);
    }

    #endregion

    #region GetTeam Tests

    [Fact]
    public async Task GetTeam_WhenTeamExists_ReturnsOkWithTeam()
    {
        // Arrange
        SetupAuthorizationMocks(canAccessTeam: true);
        var team = CreateTestTeam(1, "Falcons", "Atlanta");
        _mockTeamRepository.Setup(repo => repo.GetByIdAsync(1)).ReturnsAsync(team);

        // Act
        var result = await _controller.GetTeam(1);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var teamDto = okResult!.Value as TeamDto;
        teamDto!.Id.Should().Be(1);
        teamDto.Name.Should().Be("Falcons");
    }

    [Fact]
    public async Task GetTeam_WhenTeamNotFound_Returns404()
    {
        // Arrange
        SetupAuthorizationMocks(canAccessTeam: true);
        _mockTeamRepository.Setup(repo => repo.GetByIdAsync(999)).ReturnsAsync((Team?)null);

        // Act
        var result = await _controller.GetTeam(999);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetTeam_CallsRepositoryWithCorrectId()
    {
        // Arrange
        SetupAuthorizationMocks(canAccessTeam: true);
        var team = CreateTestTeam(42, "Test", "City");
        _mockTeamRepository.Setup(repo => repo.GetByIdAsync(42)).ReturnsAsync(team);

        // Act
        await _controller.GetTeam(42);

        // Assert
        _mockTeamRepository.Verify(repo => repo.GetByIdAsync(42), Times.Once);
    }

    #endregion

    #region GetTeamRoster Tests

    [Fact]
    public async Task GetTeamRoster_WhenTeamExists_ReturnsTeamWithPlayers()
    {
        // Arrange
        SetupAuthorizationMocks(canAccessTeam: true);
        var team = CreateTestTeamWithPlayers(1, "Falcons", "Atlanta", 3);
        _mockTeamRepository.Setup(repo => repo.GetByIdWithPlayersAsync(1)).ReturnsAsync(team);

        // Act
        var result = await _controller.GetTeamRoster(1);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var teamDetailDto = okResult!.Value as TeamDetailDto;
        teamDetailDto!.Roster.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetTeamRoster_WhenTeamNotFound_Returns404()
    {
        // Arrange
        SetupAuthorizationMocks(canAccessTeam: true);
        _mockTeamRepository.Setup(repo => repo.GetByIdWithPlayersAsync(999)).ReturnsAsync((Team?)null);

        // Act
        var result = await _controller.GetTeamRoster(999);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetTeamRoster_MapsPlayersToDtosCorrectly()
    {
        // Arrange
        SetupAuthorizationMocks(canAccessTeam: true);
        var team = CreateTestTeamWithPlayers(1, "Falcons", "Atlanta", 2);
        team.Players[0].FirstName = "Matt";
        team.Players[0].LastName = "Ryan";
        team.Players[0].Position = Positions.QB;
        team.Players[0].Number = 2;

        _mockTeamRepository.Setup(repo => repo.GetByIdWithPlayersAsync(1)).ReturnsAsync(team);

        // Act
        var result = await _controller.GetTeamRoster(1);

        // Assert
        var okResult = result.Result as OkObjectResult;
        var teamDetailDto = okResult!.Value as TeamDetailDto;
        var playerDto = teamDetailDto!.Roster.First();

        playerDto.FirstName.Should().Be("Matt");
        playerDto.LastName.Should().Be("Ryan");
        playerDto.FullName.Should().Be("Matt Ryan");
        playerDto.Position.Should().Be("QB");
        playerDto.Number.Should().Be(2);
    }

    [Fact]
    public async Task GetTeamRoster_WhenTeamHasCoach_ReturnsCoachDto()
    {
        // Arrange
        SetupAuthorizationMocks(canAccessTeam: true);
        var team = CreateTestTeamWithPlayers(1, "Falcons", "Atlanta", 1);
        team.HeadCoach = new Coach { FirstName = "Arthur", LastName = "Smith" };

        _mockTeamRepository.Setup(repo => repo.GetByIdWithPlayersAsync(1)).ReturnsAsync(team);

        // Act
        var result = await _controller.GetTeamRoster(1);

        // Assert
        var okResult = result.Result as OkObjectResult;
        var teamDetailDto = okResult!.Value as TeamDetailDto;
        teamDetailDto!.HeadCoach.Should().NotBeNull();
        teamDetailDto.HeadCoach!.FirstName.Should().Be("Arthur");
        teamDetailDto.HeadCoach.LastName.Should().Be("Smith");
    }

    [Fact]
    public async Task GetTeamRoster_WhenTeamHasNoCoach_ReturnsNullForCoach()
    {
        // Arrange
        SetupAuthorizationMocks(canAccessTeam: true);
        var team = CreateTestTeamWithPlayers(1, "Falcons", "Atlanta", 1);
        team.HeadCoach = null;

        _mockTeamRepository.Setup(repo => repo.GetByIdWithPlayersAsync(1)).ReturnsAsync(team);

        // Act
        var result = await _controller.GetTeamRoster(1);

        // Assert
        var okResult = result.Result as OkObjectResult;
        var teamDetailDto = okResult!.Value as TeamDetailDto;
        teamDetailDto!.HeadCoach.Should().BeNull();
    }

    #endregion

    #region Helper Methods

    private Team CreateTestTeam(int id, string name, string city)
    {
        return new Team
        {
            Id = id,
            Name = name,
            City = city,
            Players = new List<Player>()
        };
    }

    private Team CreateTestTeamWithPlayers(int id, string name, string city, int playerCount)
    {
        var team = CreateTestTeam(id, name, city);

        for (int i = 1; i <= playerCount; i++)
        {
            team.Players.Add(new Player
            {
                Id = i,
                FirstName = $"Player{i}",
                LastName = $"Test{i}",
                Position = Positions.QB,
                Number = i,
                TeamId = id,
                Height = "6-2",
                Weight = 200,
                Age = 25,
                Exp = 3,
                College = "Test University"
            });
        }

        return team;
    }

    #endregion
}

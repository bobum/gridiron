using System.Security.Claims;
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
using Xunit;

namespace Gridiron.WebApi.Tests.Controllers;

/// <summary>
/// Unit tests for LeaguesManagementController
/// Tests the controller logic WITHOUT touching the database (mocked repositories).
/// </summary>
public class LeaguesManagementControllerTests
{
    private readonly Mock<ILeagueRepository> _mockLeagueRepository;
    private readonly Mock<ILeagueBuilderService> _mockLeagueBuilderService;
    private readonly Mock<IGridironAuthorizationService> _mockAuthorizationService;
    private readonly Mock<ILogger<LeaguesManagementController>> _mockLogger;
    private readonly LeaguesManagementController _controller;

    public LeaguesManagementControllerTests()
    {
        _mockLeagueRepository = new Mock<ILeagueRepository>();
        _mockLeagueBuilderService = new Mock<ILeagueBuilderService>();
        _mockAuthorizationService = new Mock<IGridironAuthorizationService>();
        _mockLogger = new Mock<ILogger<LeaguesManagementController>>();
        _controller = new LeaguesManagementController(
            _mockLeagueRepository.Object,
            _mockLeagueBuilderService.Object,
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

    private void SetupAuthorizationMocks(bool canAccessLeague = true, bool isCommissioner = true, bool isGlobalAdmin = false)
    {
        var testUser = new User { Id = 1, AzureAdObjectId = "test-oid-123", Email = "test@example.com", DisplayName = "Test User" };

        _mockAuthorizationService
            .Setup(s => s.GetOrCreateUserFromClaimsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(testUser);

        _mockAuthorizationService
            .Setup(s => s.CanAccessLeagueAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(canAccessLeague);

        _mockAuthorizationService
            .Setup(s => s.IsCommissionerOfLeagueAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(isCommissioner);

        _mockAuthorizationService
            .Setup(s => s.IsGlobalAdminAsync(It.IsAny<string>()))
            .ReturnsAsync(isGlobalAdmin);

        _mockAuthorizationService
            .Setup(s => s.GetAccessibleLeagueIdsAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<int> { 1, 2, 3 });

        _mockLeagueRepository
            .Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullLeagueRepository_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new LeaguesManagementController(
            null!,
            _mockLeagueBuilderService.Object,
            _mockAuthorizationService.Object,
            _mockLogger.Object);
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("leagueRepository");
    }

    [Fact]
    public void Constructor_WithNullLeagueBuilderService_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new LeaguesManagementController(
            _mockLeagueRepository.Object,
            null!,
            _mockAuthorizationService.Object,
            _mockLogger.Object);
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("leagueBuilderService");
    }

    [Fact]
    public void Constructor_WithNullAuthorizationService_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new LeaguesManagementController(
            _mockLeagueRepository.Object,
            _mockLeagueBuilderService.Object,
            null!,
            _mockLogger.Object);
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("authorizationService");
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new LeaguesManagementController(
            _mockLeagueRepository.Object,
            _mockLeagueBuilderService.Object,
            _mockAuthorizationService.Object,
            null!);
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("logger");
    }

    #endregion

    #region CreateLeague Tests - Valid Requests

    [Fact]
    public async Task CreateLeague_WithValidRequest_ReturnsCreatedAtAction()
    {
        // Arrange
        var request = new CreateLeagueRequest
        {
            Name = "NFL",
            NumberOfConferences = 2,
            DivisionsPerConference = 4,
            TeamsPerDivision = 4
        };

        var league = CreateTestLeague(1, "NFL", 2, 4, 4);
        var testUser = new User { Id = 1, AzureAdObjectId = "test-oid-123", Email = "test@example.com", DisplayName = "Test User" };

        _mockAuthorizationService
            .Setup(s => s.GetOrCreateUserFromClaimsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(testUser);

        _mockLeagueBuilderService
            .Setup(s => s.CreateLeague(request.Name, request.NumberOfConferences,
                request.DivisionsPerConference, request.TeamsPerDivision))
            .Returns(league);

        _mockLeagueRepository
            .Setup(r => r.AddAsync(It.IsAny<League>()))
            .ReturnsAsync(league);

        _mockLeagueRepository
            .Setup(r => r.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _controller.CreateLeague(request);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result.Result as CreatedAtActionResult;
        createdResult!.ActionName.Should().Be(nameof(_controller.GetLeague));
    }

    [Fact]
    public async Task CreateLeague_WithValidRequest_ReturnsLeagueDetailDto()
    {
        // Arrange
        SetupAuthorizationMocks();
        var request = new CreateLeagueRequest
        {
            Name = "NFL",
            NumberOfConferences = 2,
            DivisionsPerConference = 4,
            TeamsPerDivision = 4
        };

        var league = CreateTestLeague(1, "NFL", 2, 4, 4);

        _mockLeagueBuilderService
            .Setup(s => s.CreateLeague(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(league);

        _mockLeagueRepository
            .Setup(r => r.AddAsync(It.IsAny<League>()))
            .ReturnsAsync(league);

        // Act
        var result = await _controller.CreateLeague(request);

        // Assert
        var createdResult = result.Result as CreatedAtActionResult;
        var leagueDto = createdResult!.Value as LeagueDetailDto;

        leagueDto.Should().NotBeNull();
        leagueDto!.Id.Should().Be(1);
        leagueDto.Name.Should().Be("NFL");
        leagueDto.TotalConferences.Should().Be(2);
        leagueDto.TotalTeams.Should().Be(32);
        leagueDto.Conferences.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateLeague_CallsServiceWithCorrectParameters()
    {
        // Arrange
        var request = new CreateLeagueRequest
        {
            Name = "Test League",
            NumberOfConferences = 3,
            DivisionsPerConference = 2,
            TeamsPerDivision = 5
        };

        var league = CreateTestLeague(1, "Test League", 3, 2, 5);

        _mockLeagueBuilderService
            .Setup(s => s.CreateLeague(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(league);

        _mockLeagueRepository
            .Setup(r => r.AddAsync(It.IsAny<League>()))
            .ReturnsAsync(league);

        // Act
        await _controller.CreateLeague(request);

        // Assert
        _mockLeagueBuilderService.Verify(
            s => s.CreateLeague("Test League", 3, 2, 5),
            Times.Once);
    }

    [Fact]
    public async Task CreateLeague_CallsRepositoryToAddLeague()
    {
        // Arrange
        var request = new CreateLeagueRequest
        {
            Name = "NFL",
            NumberOfConferences = 2,
            DivisionsPerConference = 4,
            TeamsPerDivision = 4
        };

        var league = CreateTestLeague(1, "NFL", 2, 4, 4);

        _mockLeagueBuilderService
            .Setup(s => s.CreateLeague(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(league);

        _mockLeagueRepository
            .Setup(r => r.AddAsync(It.IsAny<League>()))
            .ReturnsAsync(league);

        // Act
        await _controller.CreateLeague(request);

        // Assert
        _mockLeagueRepository.Verify(r => r.AddAsync(It.IsAny<League>()), Times.Once);
    }

    [Fact]
    public async Task CreateLeague_MapsCompleteHierarchyToDto()
    {
        // Arrange
        SetupAuthorizationMocks();
        var request = new CreateLeagueRequest
        {
            Name = "NFL",
            NumberOfConferences = 2,
            DivisionsPerConference = 2,
            TeamsPerDivision = 2
        };

        var league = CreateTestLeague(1, "NFL", 2, 2, 2);

        _mockLeagueBuilderService
            .Setup(s => s.CreateLeague(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(league);

        _mockLeagueRepository
            .Setup(r => r.AddAsync(It.IsAny<League>()))
            .ReturnsAsync(league);

        // Act
        var result = await _controller.CreateLeague(request);

        // Assert
        var createdResult = result.Result as CreatedAtActionResult;
        var leagueDto = createdResult!.Value as LeagueDetailDto;

        leagueDto!.Conferences.Should().HaveCount(2);
        leagueDto.Conferences[0].Divisions.Should().HaveCount(2);
        leagueDto.Conferences[0].Divisions[0].Teams.Should().HaveCount(2);

        // Verify team mapping
        var firstTeam = leagueDto.Conferences[0].Divisions[0].Teams[0];
        firstTeam.Name.Should().Be("Team 1");
        firstTeam.FanSupport.Should().Be(50);
        firstTeam.Chemistry.Should().Be(50);
    }

    #endregion

    #region CreateLeague Tests - Invalid Requests

    [Fact]
    public async Task CreateLeague_WithNullRequest_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.CreateLeague(null!);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task CreateLeague_WithEmptyName_ReturnsBadRequest(string? name)
    {
        // Arrange
        var request = new CreateLeagueRequest
        {
            Name = name,
            NumberOfConferences = 2,
            DivisionsPerConference = 4,
            TeamsPerDivision = 4
        };

        // Act
        var result = await _controller.CreateLeague(request);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = result.Result as BadRequestObjectResult;
        badRequest!.Value.Should().NotBeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CreateLeague_WithInvalidConferenceCount_ReturnsBadRequest(int conferences)
    {
        // Arrange
        var request = new CreateLeagueRequest
        {
            Name = "NFL",
            NumberOfConferences = conferences,
            DivisionsPerConference = 4,
            TeamsPerDivision = 4
        };

        // Act
        var result = await _controller.CreateLeague(request);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CreateLeague_WithInvalidDivisionsPerConference_ReturnsBadRequest(int divisions)
    {
        // Arrange
        var request = new CreateLeagueRequest
        {
            Name = "NFL",
            NumberOfConferences = 2,
            DivisionsPerConference = divisions,
            TeamsPerDivision = 4
        };

        // Act
        var result = await _controller.CreateLeague(request);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CreateLeague_WithInvalidTeamsPerDivision_ReturnsBadRequest(int teams)
    {
        // Arrange
        var request = new CreateLeagueRequest
        {
            Name = "NFL",
            NumberOfConferences = 2,
            DivisionsPerConference = 4,
            TeamsPerDivision = teams
        };

        // Act
        var result = await _controller.CreateLeague(request);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    #endregion

    #region CreateLeague Tests - Exception Handling

    [Fact]
    public async Task CreateLeague_WhenServiceThrowsArgumentException_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateLeagueRequest
        {
            Name = "NFL",
            NumberOfConferences = 2,
            DivisionsPerConference = 4,
            TeamsPerDivision = 4
        };

        _mockLeagueBuilderService
            .Setup(s => s.CreateLeague(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .Throws(new ArgumentException("Invalid parameter"));

        // Act
        var result = await _controller.CreateLeague(request);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CreateLeague_WhenRepositoryThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var request = new CreateLeagueRequest
        {
            Name = "NFL",
            NumberOfConferences = 2,
            DivisionsPerConference = 4,
            TeamsPerDivision = 4
        };

        var league = CreateTestLeague(1, "NFL", 2, 4, 4);

        _mockLeagueBuilderService
            .Setup(s => s.CreateLeague(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .Returns(league);

        _mockLeagueRepository
            .Setup(r => r.AddAsync(It.IsAny<League>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.CreateLeague(request);

        // Assert
        result.Result.Should().BeOfType<ObjectResult>();
        var objectResult = result.Result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }

    #endregion

    #region GetLeague Tests

    [Fact]
    public async Task GetLeague_WhenLeagueExists_ReturnsOkWithLeague()
    {
        // Arrange
        SetupAuthorizationMocks();
        var league = CreateTestLeague(1, "NFL", 2, 4, 4);
        _mockLeagueRepository
            .Setup(r => r.GetByIdWithFullStructureAsync(1))
            .ReturnsAsync(league);

        // Act
        var result = await _controller.GetLeague(1);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var leagueDto = okResult!.Value as LeagueDetailDto;
        leagueDto!.Id.Should().Be(1);
        leagueDto.Name.Should().Be("NFL");
    }

    [Fact]
    public async Task GetLeague_WhenLeagueNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockLeagueRepository
            .Setup(r => r.GetByIdWithFullStructureAsync(999))
            .ReturnsAsync((League?)null);

        // Act
        var result = await _controller.GetLeague(999);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetLeague_CallsRepositoryWithCorrectId()
    {
        // Arrange
        var league = CreateTestLeague(42, "Test", 1, 1, 1);
        _mockLeagueRepository
            .Setup(r => r.GetByIdWithFullStructureAsync(42))
            .ReturnsAsync(league);

        // Act
        await _controller.GetLeague(42);

        // Assert
        _mockLeagueRepository.Verify(r => r.GetByIdWithFullStructureAsync(42), Times.Once);
    }

    [Fact]
    public async Task GetLeague_WhenRepositoryThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        _mockLeagueRepository
            .Setup(r => r.GetByIdWithFullStructureAsync(It.IsAny<int>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetLeague(1);

        // Assert
        result.Result.Should().BeOfType<ObjectResult>();
        var objectResult = result.Result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }

    #endregion

    #region GetAllLeagues Tests

    [Fact]
    public async Task GetAllLeagues_WhenLeaguesExist_ReturnsOkWithLeagues()
    {
        // Arrange
        SetupAuthorizationMocks();
        var leagues = new List<League>
        {
            CreateTestLeague(1, "NFL", 2, 4, 4),
            CreateTestLeague(2, "CFL", 2, 3, 3)
        };
        _mockLeagueRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(leagues);

        // Act
        var result = await _controller.GetAllLeagues();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var leagueDtos = okResult!.Value as List<LeagueDto>;
        leagueDtos.Should().HaveCount(2);
        leagueDtos![0].Name.Should().Be("NFL");
        leagueDtos[1].Name.Should().Be("CFL");
    }

    [Fact]
    public async Task GetAllLeagues_WhenNoLeagues_ReturnsEmptyList()
    {
        // Arrange
        _mockLeagueRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<League>());

        // Act
        var result = await _controller.GetAllLeagues();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var leagueDtos = okResult!.Value as List<LeagueDto>;
        leagueDtos.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllLeagues_WhenRepositoryThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        _mockLeagueRepository
            .Setup(r => r.GetAllAsync())
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetAllLeagues();

        // Assert
        result.Result.Should().BeOfType<ObjectResult>();
        var objectResult = result.Result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }

    #endregion

    #region PopulateLeagueRosters Tests

    [Fact]
    public async Task PopulateLeagueRosters_WhenLeagueExists_ReturnsOkWithLeague()
    {
        // Arrange
        SetupAuthorizationMocks();
        var league = CreateTestLeague(1, "NFL", 2, 2, 2);
        _mockLeagueRepository
            .Setup(r => r.GetByIdWithFullStructureAsync(1))
            .ReturnsAsync(league);

        _mockLeagueBuilderService
            .Setup(s => s.PopulateLeagueRosters(league, It.IsAny<int?>()))
            .Returns(league);

        _mockLeagueRepository
            .Setup(r => r.UpdateAsync(league))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.PopulateLeagueRosters(1, null);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var leagueDto = okResult!.Value as LeagueDto;
        leagueDto!.Id.Should().Be(1);
        leagueDto.Name.Should().Be("NFL");
    }

    [Fact]
    public async Task PopulateLeagueRosters_WhenLeagueNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockLeagueRepository
            .Setup(r => r.GetByIdWithFullStructureAsync(999))
            .ReturnsAsync((League?)null);

        // Act
        var result = await _controller.PopulateLeagueRosters(999, null);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task PopulateLeagueRosters_WithSeed_PassesSeedToService()
    {
        // Arrange
        SetupAuthorizationMocks();
        var league = CreateTestLeague(1, "NFL", 1, 1, 1);
        var seed = 12345;

        _mockLeagueRepository
            .Setup(r => r.GetByIdWithFullStructureAsync(1))
            .ReturnsAsync(league);

        _mockLeagueBuilderService
            .Setup(s => s.PopulateLeagueRosters(league, seed))
            .Returns(league);

        _mockLeagueRepository
            .Setup(r => r.UpdateAsync(league))
            .Returns(Task.CompletedTask);

        // Act
        await _controller.PopulateLeagueRosters(1, seed);

        // Assert
        _mockLeagueBuilderService.Verify(
            s => s.PopulateLeagueRosters(league, seed),
            Times.Once);
    }

    [Fact]
    public async Task PopulateLeagueRosters_CallsRepositoryUpdate()
    {
        // Arrange
        SetupAuthorizationMocks();
        var league = CreateTestLeague(1, "NFL", 1, 1, 1);

        _mockLeagueRepository
            .Setup(r => r.GetByIdWithFullStructureAsync(1))
            .ReturnsAsync(league);

        _mockLeagueBuilderService
            .Setup(s => s.PopulateLeagueRosters(league, It.IsAny<int?>()))
            .Returns(league);

        _mockLeagueRepository
            .Setup(r => r.UpdateAsync(league))
            .Returns(Task.CompletedTask);

        // Act
        await _controller.PopulateLeagueRosters(1, null);

        // Assert
        _mockLeagueRepository.Verify(r => r.UpdateAsync(league), Times.Once);
    }

    [Fact]
    public async Task PopulateLeagueRosters_WhenServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        SetupAuthorizationMocks();
        var league = CreateTestLeague(1, "NFL", 1, 1, 1);

        _mockLeagueRepository
            .Setup(r => r.GetByIdWithFullStructureAsync(1))
            .ReturnsAsync(league);

        _mockLeagueBuilderService
            .Setup(s => s.PopulateLeagueRosters(It.IsAny<League>(), It.IsAny<int?>()))
            .Throws(new Exception("Population error"));

        // Act
        var result = await _controller.PopulateLeagueRosters(1, null);

        // Assert
        result.Result.Should().BeOfType<ObjectResult>();
        var objectResult = result.Result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }

    #endregion

    #region Helper Methods

    private League CreateTestLeague(int id, string name, int conferences, int divisionsPerConf, int teamsPerDiv)
    {
        var league = new League
        {
            Id = id,
            Name = name,
            Season = DateTime.Now.Year,
            IsActive = true,
            Conferences = new List<Conference>()
        };

        for (int c = 1; c <= conferences; c++)
        {
            var conference = new Conference
            {
                Id = c,
                Name = $"Conference {c}",
                LeagueId = id,
                Divisions = new List<Division>()
            };

            for (int d = 1; d <= divisionsPerConf; d++)
            {
                var division = new Division
                {
                    Id = ((c - 1) * divisionsPerConf) + d,
                    Name = $"Division {d}",
                    ConferenceId = c,
                    Teams = new List<Team>()
                };

                for (int t = 1; t <= teamsPerDiv; t++)
                {
                    var team = new Team
                    {
                        Id = ((c - 1) * divisionsPerConf * teamsPerDiv) + ((d - 1) * teamsPerDiv) + t,
                        Name = $"Team {t}",
                        City = $"City {c}-{d}-{t}",
                        DivisionId = division.Id,
                        Players = new List<Player>(),
                        Budget = 0,
                        Championships = 0,
                        Wins = 0,
                        Losses = 0,
                        Ties = 0,
                        FanSupport = 50,
                        Chemistry = 50
                    };
                    division.Teams.Add(team);
                }

                conference.Divisions.Add(division);
            }

            league.Conferences.Add(conference);
        }

        return league;
    }

    #endregion
}

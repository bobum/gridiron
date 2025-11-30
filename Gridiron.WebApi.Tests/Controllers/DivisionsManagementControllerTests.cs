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
/// Unit tests for DivisionsManagementController
/// Tests the controller logic WITHOUT touching the database (mocked repositories)
/// </summary>
public class DivisionsManagementControllerTests
{
    private readonly Mock<IDivisionRepository> _mockDivisionRepository;
    private readonly Mock<IConferenceRepository> _mockConferenceRepository;
    private readonly Mock<IDivisionBuilderService> _mockDivisionBuilderService;
    private readonly Mock<IGridironAuthorizationService> _mockAuthorizationService;
    private readonly Mock<ILogger<DivisionsManagementController>> _mockLogger;
    private readonly DivisionsManagementController _controller;

    public DivisionsManagementControllerTests()
    {
        _mockDivisionRepository = new Mock<IDivisionRepository>();
        _mockConferenceRepository = new Mock<IConferenceRepository>();
        _mockDivisionBuilderService = new Mock<IDivisionBuilderService>();
        _mockAuthorizationService = new Mock<IGridironAuthorizationService>();
        _mockLogger = new Mock<ILogger<DivisionsManagementController>>();
        _controller = new DivisionsManagementController(
            _mockDivisionRepository.Object,
            _mockConferenceRepository.Object,
            _mockDivisionBuilderService.Object,
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

    private void SetupHttpContextWithoutClaims()
    {
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity()) // No claims
        };

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };
    }

    private void SetupAuthorizationMocks(bool canAccessLeague = true, bool isCommissioner = true, bool isGlobalAdmin = false)
    {
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
    }

    private static Division CreateTestDivision(int id, string name, int conferenceId)
    {
        return new Division
        {
            Id = id,
            Name = name,
            ConferenceId = conferenceId,
            Teams = new List<Team>
            {
                new Team { Id = 1, Name = "Team1", City = "City1" },
                new Team { Id = 2, Name = "Team2", City = "City2" }
            }
        };
    }

    private static Conference CreateTestConference(int id, string name, int leagueId)
    {
        return new Conference
        {
            Id = id,
            Name = name,
            LeagueId = leagueId
        };
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullDivisionRepository_ThrowsArgumentNullException()
    {
        var act = () => new DivisionsManagementController(
            null!,
            _mockConferenceRepository.Object,
            _mockDivisionBuilderService.Object,
            _mockAuthorizationService.Object,
            _mockLogger.Object);
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("divisionRepository");
    }

    [Fact]
    public void Constructor_WithNullConferenceRepository_ThrowsArgumentNullException()
    {
        var act = () => new DivisionsManagementController(
            _mockDivisionRepository.Object,
            null!,
            _mockDivisionBuilderService.Object,
            _mockAuthorizationService.Object,
            _mockLogger.Object);
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("conferenceRepository");
    }

    [Fact]
    public void Constructor_WithNullDivisionBuilderService_ThrowsArgumentNullException()
    {
        var act = () => new DivisionsManagementController(
            _mockDivisionRepository.Object,
            _mockConferenceRepository.Object,
            null!,
            _mockAuthorizationService.Object,
            _mockLogger.Object);
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("divisionBuilderService");
    }

    [Fact]
    public void Constructor_WithNullAuthorizationService_ThrowsArgumentNullException()
    {
        var act = () => new DivisionsManagementController(
            _mockDivisionRepository.Object,
            _mockConferenceRepository.Object,
            _mockDivisionBuilderService.Object,
            null!,
            _mockLogger.Object);
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("authorizationService");
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        var act = () => new DivisionsManagementController(
            _mockDivisionRepository.Object,
            _mockConferenceRepository.Object,
            _mockDivisionBuilderService.Object,
            _mockAuthorizationService.Object,
            null!);
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("logger");
    }

    #endregion

    #region GetDivision Tests

    [Fact]
    public async Task GetDivision_WhenDivisionExistsAndUserHasAccess_ReturnsOkWithDivision()
    {
        // Arrange
        var division = CreateTestDivision(1, "NFC East", 1);
        var conference = CreateTestConference(1, "NFC", 1);

        _mockDivisionRepository
            .Setup(r => r.GetByIdWithTeamsAsync(1))
            .ReturnsAsync(division);

        _mockConferenceRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(conference);

        SetupAuthorizationMocks(canAccessLeague: true);

        // Act
        var result = await _controller.GetDivision(1);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var divisionDto = okResult!.Value as DivisionDto;
        divisionDto.Should().NotBeNull();
        divisionDto!.Id.Should().Be(1);
        divisionDto.Name.Should().Be("NFC East");
        divisionDto.Teams.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetDivision_WhenDivisionNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockDivisionRepository
            .Setup(r => r.GetByIdWithTeamsAsync(999))
            .ReturnsAsync((Division?)null);

        // Act
        var result = await _controller.GetDivision(999);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetDivision_WhenConferenceNotFound_ReturnsNotFound()
    {
        // Arrange
        var division = CreateTestDivision(1, "NFC East", 999);

        _mockDivisionRepository
            .Setup(r => r.GetByIdWithTeamsAsync(1))
            .ReturnsAsync(division);

        _mockConferenceRepository
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Conference?)null);

        // Act
        var result = await _controller.GetDivision(1);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetDivision_WhenUserDoesNotHaveAccess_ReturnsForbid()
    {
        // Arrange
        var division = CreateTestDivision(1, "NFC East", 1);
        var conference = CreateTestConference(1, "NFC", 1);

        _mockDivisionRepository
            .Setup(r => r.GetByIdWithTeamsAsync(1))
            .ReturnsAsync(division);

        _mockConferenceRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(conference);

        SetupAuthorizationMocks(canAccessLeague: false);

        // Act
        var result = await _controller.GetDivision(1);

        // Assert
        result.Result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task GetDivision_WhenNoUserIdentity_ReturnsUnauthorized()
    {
        // Arrange
        SetupHttpContextWithoutClaims();

        // Act
        var result = await _controller.GetDivision(1);

        // Assert
        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task GetDivision_WhenExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        _mockDivisionRepository
            .Setup(r => r.GetByIdWithTeamsAsync(It.IsAny<int>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetDivision(1);

        // Assert
        result.Result.Should().BeOfType<ObjectResult>();
        var objectResult = result.Result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }

    #endregion

    #region GetAllDivisions Tests

    [Fact]
    public async Task GetAllDivisions_WhenUserIsGlobalAdmin_ReturnsAllDivisions()
    {
        // Arrange
        var divisions = new List<Division>
        {
            CreateTestDivision(1, "NFC East", 1),
            CreateTestDivision(2, "NFC West", 1),
            CreateTestDivision(3, "AFC East", 2)
        };

        var conferences = new List<Conference>
        {
            CreateTestConference(1, "NFC", 1),
            CreateTestConference(2, "AFC", 1)
        };

        _mockDivisionRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(divisions);

        _mockConferenceRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(conferences);

        SetupAuthorizationMocks(isGlobalAdmin: true);

        // Act
        var result = await _controller.GetAllDivisions();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var divisionDtos = okResult!.Value as List<DivisionDto>;
        divisionDtos.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetAllDivisions_WhenRegularUser_ReturnsFilteredDivisions()
    {
        // Arrange
        var divisions = new List<Division>
        {
            CreateTestDivision(1, "NFC East", 1),
            CreateTestDivision(2, "NFC West", 1),
            CreateTestDivision(3, "AFC East", 2)
        };

        var conferences = new List<Conference>
        {
            CreateTestConference(1, "NFC", 1),
            CreateTestConference(2, "AFC", 99) // League 99 not accessible
        };

        _mockDivisionRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(divisions);

        _mockConferenceRepository
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(conferences);

        _mockAuthorizationService
            .Setup(s => s.IsGlobalAdminAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        _mockAuthorizationService
            .Setup(s => s.GetAccessibleLeagueIdsAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<int> { 1 }); // Only has access to league 1

        // Act
        var result = await _controller.GetAllDivisions();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var divisionDtos = okResult!.Value as List<DivisionDto>;
        divisionDtos.Should().HaveCount(2); // Only divisions from conferences in league 1
    }

    [Fact]
    public async Task GetAllDivisions_WhenNoUserIdentity_ReturnsUnauthorized()
    {
        // Arrange
        SetupHttpContextWithoutClaims();

        // Act
        var result = await _controller.GetAllDivisions();

        // Assert
        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task GetAllDivisions_WhenExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        _mockDivisionRepository
            .Setup(r => r.GetAllAsync())
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.GetAllDivisions();

        // Assert
        result.Result.Should().BeOfType<ObjectResult>();
        var objectResult = result.Result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }

    #endregion

    #region UpdateDivision Tests

    [Fact]
    public async Task UpdateDivision_WhenCommissionerUpdates_ReturnsOkWithUpdatedDivision()
    {
        // Arrange
        var division = CreateTestDivision(1, "NFC East", 1);
        var conference = CreateTestConference(1, "NFC", 1);
        var request = new UpdateDivisionRequest { Name = "NFC East Updated" };

        _mockDivisionRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(division);

        _mockConferenceRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(conference);

        SetupAuthorizationMocks(isCommissioner: true, isGlobalAdmin: false);

        // Act
        var result = await _controller.UpdateDivision(1, request);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        _mockDivisionBuilderService.Verify(s => s.UpdateDivision(division, "NFC East Updated"), Times.Once);
        _mockDivisionRepository.Verify(r => r.UpdateAsync(division), Times.Once);
    }

    [Fact]
    public async Task UpdateDivision_WhenGlobalAdminUpdates_ReturnsOkWithUpdatedDivision()
    {
        // Arrange
        var division = CreateTestDivision(1, "NFC East", 1);
        var conference = CreateTestConference(1, "NFC", 1);
        var request = new UpdateDivisionRequest { Name = "NFC East Updated" };

        _mockDivisionRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(division);

        _mockConferenceRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(conference);

        SetupAuthorizationMocks(isCommissioner: false, isGlobalAdmin: true);

        // Act
        var result = await _controller.UpdateDivision(1, request);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task UpdateDivision_WhenRequestIsNull_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.UpdateDivision(1, null!);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UpdateDivision_WhenDivisionNotFound_ReturnsNotFound()
    {
        // Arrange
        var request = new UpdateDivisionRequest { Name = "NFC East Updated" };

        _mockDivisionRepository
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Division?)null);

        // Act
        var result = await _controller.UpdateDivision(999, request);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UpdateDivision_WhenConferenceNotFound_ReturnsNotFound()
    {
        // Arrange
        var division = CreateTestDivision(1, "NFC East", 999);
        var request = new UpdateDivisionRequest { Name = "NFC East Updated" };

        _mockDivisionRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(division);

        _mockConferenceRepository
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Conference?)null);

        // Act
        var result = await _controller.UpdateDivision(1, request);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UpdateDivision_WhenUserIsNotCommissioner_ReturnsForbid()
    {
        // Arrange
        var division = CreateTestDivision(1, "NFC East", 1);
        var conference = CreateTestConference(1, "NFC", 1);
        var request = new UpdateDivisionRequest { Name = "NFC East Updated" };

        _mockDivisionRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(division);

        _mockConferenceRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(conference);

        SetupAuthorizationMocks(isCommissioner: false, isGlobalAdmin: false);

        // Act
        var result = await _controller.UpdateDivision(1, request);

        // Assert
        result.Result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task UpdateDivision_WhenNoUserIdentity_ReturnsUnauthorized()
    {
        // Arrange
        SetupHttpContextWithoutClaims();
        var request = new UpdateDivisionRequest { Name = "NFC East Updated" };

        // Act
        var result = await _controller.UpdateDivision(1, request);

        // Assert
        result.Result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task UpdateDivision_WhenBuilderServiceThrowsArgumentException_ReturnsBadRequest()
    {
        // Arrange
        var division = CreateTestDivision(1, "NFC East", 1);
        var conference = CreateTestConference(1, "NFC", 1);
        var request = new UpdateDivisionRequest { Name = "" };

        _mockDivisionRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(division);

        _mockConferenceRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(conference);

        SetupAuthorizationMocks(isCommissioner: true);

        _mockDivisionBuilderService
            .Setup(s => s.UpdateDivision(It.IsAny<Division>(), It.IsAny<string?>()))
            .Throws(new ArgumentException("Name cannot be empty"));

        // Act
        var result = await _controller.UpdateDivision(1, request);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UpdateDivision_WhenExceptionThrown_ReturnsInternalServerError()
    {
        // Arrange
        var request = new UpdateDivisionRequest { Name = "NFC East Updated" };

        _mockDivisionRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _controller.UpdateDivision(1, request);

        // Assert
        result.Result.Should().BeOfType<ObjectResult>();
        var objectResult = result.Result as ObjectResult;
        objectResult!.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task UpdateDivision_ReturnsCorrectDtoStructure()
    {
        // Arrange
        var division = CreateTestDivision(1, "NFC East", 1);
        var conference = CreateTestConference(1, "NFC", 1);
        var request = new UpdateDivisionRequest { Name = "NFC East Updated" };

        _mockDivisionRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(division);

        _mockConferenceRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(conference);

        SetupAuthorizationMocks(isCommissioner: true);

        // Act
        var result = await _controller.UpdateDivision(1, request);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var divisionDto = okResult!.Value as DivisionDto;
        divisionDto.Should().NotBeNull();
        divisionDto!.Id.Should().Be(1);
        divisionDto.Teams.Should().BeEmpty(); // Teams not loaded for update response
    }

    #endregion
}

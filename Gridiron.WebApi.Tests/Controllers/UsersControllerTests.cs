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
/// Unit tests for UsersController
/// Tests the controller logic WITHOUT touching the database (mocked repositories).
/// </summary>
public class UsersControllerTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<ILeagueRepository> _mockLeagueRepository;
    private readonly Mock<ITeamRepository> _mockTeamRepository;
    private readonly Mock<IDivisionRepository> _mockDivisionRepository;
    private readonly Mock<IConferenceRepository> _mockConferenceRepository;
    private readonly Mock<IGridironAuthorizationService> _mockAuthorizationService;
    private readonly Mock<ILogger<UsersController>> _mockLogger;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockLeagueRepository = new Mock<ILeagueRepository>();
        _mockTeamRepository = new Mock<ITeamRepository>();
        _mockDivisionRepository = new Mock<IDivisionRepository>();
        _mockConferenceRepository = new Mock<IConferenceRepository>();
        _mockAuthorizationService = new Mock<IGridironAuthorizationService>();
        _mockLogger = new Mock<ILogger<UsersController>>();
        _controller = new UsersController(
            _mockUserRepository.Object,
            _mockLeagueRepository.Object,
            _mockTeamRepository.Object,
            _mockDivisionRepository.Object,
            _mockConferenceRepository.Object,
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

    private User CreateTestUser(int id, string oid, string email, string displayName, bool isGlobalAdmin = false)
    {
        return new User
        {
            Id = id,
            AzureAdObjectId = oid,
            Email = email,
            DisplayName = displayName,
            IsGlobalAdmin = isGlobalAdmin,
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            LastLoginAt = DateTime.UtcNow,
            LeagueRoles = new List<UserLeagueRole>()
        };
    }

    private League CreateTestLeague(int id, string name)
    {
        return new League
        {
            Id = id,
            Name = name,
            Season = 2025,
            IsActive = true
        };
    }

    private Team CreateTestTeam(int id, string name, int? divisionId = null)
    {
        return new Team
        {
            Id = id,
            Name = name,
            City = "Test City",
            DivisionId = divisionId,
            Budget = 100000000,
            Championships = 0,
            Wins = 0,
            Losses = 0,
            Ties = 0,
            FanSupport = 50,
            Chemistry = 50
        };
    }

    #region GetCurrentUser Tests

    [Fact]
    public async Task GetCurrentUser_WhenUserExists_ReturnsOkWithUserDto()
    {
        // Arrange
        var user = CreateTestUser(1, "test-oid-123", "test@example.com", "Test User");
        _mockUserRepository
            .Setup(repo => repo.GetByAzureAdObjectIdWithRolesAsync("test-oid-123"))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.GetCurrentUser();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var userDto = okResult.Value.Should().BeOfType<UserDto>().Subject;
        userDto.Id.Should().Be(1);
        userDto.Email.Should().Be("test@example.com");
        userDto.DisplayName.Should().Be("Test User");
    }

    [Fact]
    public async Task GetCurrentUser_WhenUserNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockUserRepository
            .Setup(repo => repo.GetByAzureAdObjectIdWithRolesAsync("test-oid-123"))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _controller.GetCurrentUser();

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetCurrentUser_WithLeagueRoles_IncludesRolesInDto()
    {
        // Arrange
        var league = CreateTestLeague(1, "Test League");
        var team = CreateTestTeam(1, "Test Team");
        var user = CreateTestUser(1, "test-oid-123", "test@example.com", "Test User");
        user.LeagueRoles.Add(new UserLeagueRole
        {
            Id = 1,
            UserId = 1,
            LeagueId = 1,
            Role = UserRole.GeneralManager,
            TeamId = 1,
            League = league,
            Team = team,
            AssignedAt = DateTime.UtcNow
        });

        _mockUserRepository
            .Setup(repo => repo.GetByAzureAdObjectIdWithRolesAsync("test-oid-123"))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.GetCurrentUser();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var userDto = okResult.Value.Should().BeOfType<UserDto>().Subject;
        userDto.LeagueRoles.Should().HaveCount(1);
        userDto.LeagueRoles[0].Role.Should().Be("GeneralManager");
        userDto.LeagueRoles[0].LeagueName.Should().Be("Test League");
        userDto.LeagueRoles[0].TeamName.Should().Be("Test Team");
    }

    #endregion

    #region GetUsersByLeague Tests

    [Fact]
    public async Task GetUsersByLeague_WhenLeagueNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockLeagueRepository
            .Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync((League?)null);

        // Act
        var result = await _controller.GetUsersByLeague(1);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetUsersByLeague_WhenUserLacksAccess_ReturnsForbid()
    {
        // Arrange
        var league = CreateTestLeague(1, "Test League");
        _mockLeagueRepository
            .Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(league);
        _mockAuthorizationService
            .Setup(s => s.CanAccessLeagueAsync("test-oid-123", 1))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.GetUsersByLeague(1);

        // Assert
        result.Result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task GetUsersByLeague_WhenAuthorized_ReturnsUsersInLeague()
    {
        // Arrange
        var league = CreateTestLeague(1, "Test League");
        var user1 = CreateTestUser(1, "oid-1", "user1@example.com", "User 1");
        var user2 = CreateTestUser(2, "oid-2", "user2@example.com", "User 2");
        var user3 = CreateTestUser(3, "oid-3", "user3@example.com", "User 3");

        user1.LeagueRoles.Add(new UserLeagueRole
        {
            Id = 1,
            UserId = 1,
            LeagueId = 1,
            Role = UserRole.Commissioner,
            League = league,
            AssignedAt = DateTime.UtcNow
        });

        user2.LeagueRoles.Add(new UserLeagueRole
        {
            Id = 2,
            UserId = 2,
            LeagueId = 1,
            Role = UserRole.GeneralManager,
            TeamId = 1,
            League = league,
            AssignedAt = DateTime.UtcNow
        });

        // User 3 has role in a different league
        user3.LeagueRoles.Add(new UserLeagueRole
        {
            Id = 3,
            UserId = 3,
            LeagueId = 2,
            Role = UserRole.GeneralManager,
            TeamId = 2,
            AssignedAt = DateTime.UtcNow
        });

        _mockLeagueRepository
            .Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(league);
        _mockAuthorizationService
            .Setup(s => s.CanAccessLeagueAsync("test-oid-123", 1))
            .ReturnsAsync(true);
        _mockUserRepository
            .Setup(repo => repo.GetAllAsync())
            .ReturnsAsync(new List<User> { user1, user2, user3 });

        // Act
        var result = await _controller.GetUsersByLeague(1);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var userDtos = okResult.Value.Should().BeAssignableTo<IEnumerable<UserDto>>().Subject.ToList();
        userDtos.Should().HaveCount(2);
        userDtos.Should().Contain(u => u.Email == "user1@example.com");
        userDtos.Should().Contain(u => u.Email == "user2@example.com");
        userDtos.Should().NotContain(u => u.Email == "user3@example.com");
    }

    #endregion

    #region AssignLeagueRole Tests

    [Fact]
    public async Task AssignLeagueRole_WithInvalidRole_ReturnsBadRequest()
    {
        // Arrange
        var request = new AssignLeagueRoleRequest
        {
            LeagueId = 1,
            Role = "InvalidRole"
        };

        // Act
        var result = await _controller.AssignLeagueRole(1, request);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task AssignLeagueRole_GeneralManagerWithoutTeamId_ReturnsBadRequest()
    {
        // Arrange
        var request = new AssignLeagueRoleRequest
        {
            LeagueId = 1,
            Role = "GeneralManager",
            TeamId = null
        };

        // Act
        var result = await _controller.AssignLeagueRole(1, request);

        // Assert
        var badRequest = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.Value.Should().BeEquivalentTo(new { error = "TeamId is required for GeneralManager role" });
    }

    [Fact]
    public async Task AssignLeagueRole_WhenUserNotFound_ReturnsNotFound()
    {
        // Arrange
        var request = new AssignLeagueRoleRequest
        {
            LeagueId = 1,
            Role = "Commissioner"
        };

        _mockUserRepository
            .Setup(repo => repo.GetByIdWithRolesAsync(999))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _controller.AssignLeagueRole(999, request);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task AssignLeagueRole_WhenLeagueNotFound_ReturnsNotFound()
    {
        // Arrange
        var user = CreateTestUser(1, "oid-1", "user1@example.com", "User 1");
        var request = new AssignLeagueRoleRequest
        {
            LeagueId = 999,
            Role = "Commissioner"
        };

        _mockUserRepository
            .Setup(repo => repo.GetByIdWithRolesAsync(1))
            .ReturnsAsync(user);
        _mockLeagueRepository
            .Setup(repo => repo.GetByIdAsync(999))
            .ReturnsAsync((League?)null);

        // Act
        var result = await _controller.AssignLeagueRole(1, request);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task AssignLeagueRole_WhenTeamNotFound_ReturnsNotFound()
    {
        // Arrange
        var user = CreateTestUser(1, "oid-1", "user1@example.com", "User 1");
        var league = CreateTestLeague(1, "Test League");
        var request = new AssignLeagueRoleRequest
        {
            LeagueId = 1,
            Role = "GeneralManager",
            TeamId = 999
        };

        _mockUserRepository
            .Setup(repo => repo.GetByIdWithRolesAsync(1))
            .ReturnsAsync(user);
        _mockLeagueRepository
            .Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(league);
        _mockTeamRepository
            .Setup(repo => repo.GetByIdAsync(999))
            .ReturnsAsync((Team?)null);

        // Act
        var result = await _controller.AssignLeagueRole(1, request);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task AssignLeagueRole_WhenTeamNotInLeague_ReturnsBadRequest()
    {
        // Arrange
        var user = CreateTestUser(1, "oid-1", "user1@example.com", "User 1");
        var league = CreateTestLeague(1, "Test League");
        var team = CreateTestTeam(1, "Test Team", divisionId: 1);
        var division = new Division { Id = 1, Name = "Test Division", ConferenceId = 1 };
        var conference = new Conference { Id = 1, Name = "Test Conference", LeagueId = 2 }; // Different league!

        var request = new AssignLeagueRoleRequest
        {
            LeagueId = 1,
            Role = "GeneralManager",
            TeamId = 1
        };

        _mockUserRepository
            .Setup(repo => repo.GetByIdWithRolesAsync(1))
            .ReturnsAsync(user);
        _mockLeagueRepository
            .Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(league);
        _mockTeamRepository
            .Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(team);
        _mockDivisionRepository
            .Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(division);
        _mockConferenceRepository
            .Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(conference);

        // Act
        var result = await _controller.AssignLeagueRole(1, request);

        // Assert
        var badRequest = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.Value.Should().BeEquivalentTo(new { error = "Team 1 does not belong to League 1" });
    }

    [Fact]
    public async Task AssignLeagueRole_WhenNotAuthorized_ReturnsForbid()
    {
        // Arrange
        var user = CreateTestUser(1, "oid-1", "user1@example.com", "User 1");
        var league = CreateTestLeague(1, "Test League");
        var request = new AssignLeagueRoleRequest
        {
            LeagueId = 1,
            Role = "Commissioner"
        };

        _mockUserRepository
            .Setup(repo => repo.GetByIdWithRolesAsync(1))
            .ReturnsAsync(user);
        _mockLeagueRepository
            .Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(league);
        _mockAuthorizationService
            .Setup(s => s.IsGlobalAdminAsync("test-oid-123"))
            .ReturnsAsync(false);
        _mockAuthorizationService
            .Setup(s => s.IsCommissionerOfLeagueAsync("test-oid-123", 1))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.AssignLeagueRole(1, request);

        // Assert
        result.Result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task AssignLeagueRole_WhenRoleAlreadyExists_ReturnsBadRequest()
    {
        // Arrange
        var user = CreateTestUser(1, "oid-1", "user1@example.com", "User 1");
        var league = CreateTestLeague(1, "Test League");
        user.LeagueRoles.Add(new UserLeagueRole
        {
            Id = 1,
            UserId = 1,
            LeagueId = 1,
            Role = UserRole.Commissioner,
            AssignedAt = DateTime.UtcNow
        });

        var request = new AssignLeagueRoleRequest
        {
            LeagueId = 1,
            Role = "Commissioner"
        };

        _mockUserRepository
            .Setup(repo => repo.GetByIdWithRolesAsync(1))
            .ReturnsAsync(user);
        _mockLeagueRepository
            .Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(league);
        _mockAuthorizationService
            .Setup(s => s.IsGlobalAdminAsync("test-oid-123"))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.AssignLeagueRole(1, request);

        // Assert
        var badRequest = result.Result.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.Value.Should().BeEquivalentTo(new { error = "User already has this role in the league" });
    }

    [Fact]
    public async Task AssignLeagueRole_AsGlobalAdmin_SuccessfullyAssignsCommissioner()
    {
        // Arrange
        var user = CreateTestUser(1, "oid-1", "user1@example.com", "User 1");
        var league = CreateTestLeague(1, "Test League");
        var request = new AssignLeagueRoleRequest
        {
            LeagueId = 1,
            Role = "Commissioner"
        };

        _mockUserRepository
            .Setup(repo => repo.GetByIdWithRolesAsync(1))
            .ReturnsAsync(user);
        _mockLeagueRepository
            .Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(league);
        _mockAuthorizationService
            .Setup(s => s.IsGlobalAdminAsync("test-oid-123"))
            .ReturnsAsync(true);
        _mockUserRepository
            .Setup(repo => repo.UpdateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.AssignLeagueRole(1, request);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        _mockUserRepository.Verify(
            repo => repo.UpdateAsync(It.Is<User>(u =>
            u.LeagueRoles.Any(lr => lr.Role == UserRole.Commissioner && lr.LeagueId == 1))), Times.Once);
    }

    [Fact]
    public async Task AssignLeagueRole_AsCommissioner_SuccessfullyAssignsGeneralManager()
    {
        // Arrange
        var user = CreateTestUser(1, "oid-1", "user1@example.com", "User 1");
        var league = CreateTestLeague(1, "Test League");
        var team = CreateTestTeam(1, "Test Team", divisionId: 1);
        var division = new Division { Id = 1, Name = "Test Division", ConferenceId = 1 };
        var conference = new Conference { Id = 1, Name = "Test Conference", LeagueId = 1 };

        var request = new AssignLeagueRoleRequest
        {
            LeagueId = 1,
            Role = "GeneralManager",
            TeamId = 1
        };

        _mockUserRepository
            .Setup(repo => repo.GetByIdWithRolesAsync(1))
            .ReturnsAsync(user);
        _mockLeagueRepository
            .Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(league);
        _mockTeamRepository
            .Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(team);
        _mockDivisionRepository
            .Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(division);
        _mockConferenceRepository
            .Setup(repo => repo.GetByIdAsync(1))
            .ReturnsAsync(conference);
        _mockAuthorizationService
            .Setup(s => s.IsGlobalAdminAsync("test-oid-123"))
            .ReturnsAsync(false);
        _mockAuthorizationService
            .Setup(s => s.IsCommissionerOfLeagueAsync("test-oid-123", 1))
            .ReturnsAsync(true);
        _mockUserRepository
            .Setup(repo => repo.UpdateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.AssignLeagueRole(1, request);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        _mockUserRepository.Verify(
            repo => repo.UpdateAsync(It.Is<User>(u =>
            u.LeagueRoles.Any(lr => lr.Role == UserRole.GeneralManager && lr.TeamId == 1))), Times.Once);
    }

    #endregion

    #region RemoveLeagueRole Tests

    [Fact]
    public async Task RemoveLeagueRole_WhenUserNotFound_ReturnsNotFound()
    {
        // Arrange
        _mockUserRepository
            .Setup(repo => repo.GetByIdWithRolesAsync(999))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _controller.RemoveLeagueRole(999, 1);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task RemoveLeagueRole_WhenRoleNotFound_ReturnsNotFound()
    {
        // Arrange
        var user = CreateTestUser(1, "oid-1", "user1@example.com", "User 1");
        _mockUserRepository
            .Setup(repo => repo.GetByIdWithRolesAsync(1))
            .ReturnsAsync(user);

        // Act
        var result = await _controller.RemoveLeagueRole(1, 999);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task RemoveLeagueRole_WhenNotAuthorized_ReturnsForbid()
    {
        // Arrange
        var user = CreateTestUser(1, "oid-1", "user1@example.com", "User 1");
        user.LeagueRoles.Add(new UserLeagueRole
        {
            Id = 1,
            UserId = 1,
            LeagueId = 1,
            Role = UserRole.GeneralManager,
            TeamId = 1,
            AssignedAt = DateTime.UtcNow
        });

        _mockUserRepository
            .Setup(repo => repo.GetByIdWithRolesAsync(1))
            .ReturnsAsync(user);
        _mockAuthorizationService
            .Setup(s => s.IsGlobalAdminAsync("test-oid-123"))
            .ReturnsAsync(false);
        _mockAuthorizationService
            .Setup(s => s.IsCommissionerOfLeagueAsync("test-oid-123", 1))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.RemoveLeagueRole(1, 1);

        // Assert
        result.Result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task RemoveLeagueRole_AsGlobalAdmin_SuccessfullySoftDeletesRole()
    {
        // Arrange
        var user = CreateTestUser(1, "oid-1", "user1@example.com", "User 1");
        user.LeagueRoles.Add(new UserLeagueRole
        {
            Id = 1,
            UserId = 1,
            LeagueId = 1,
            Role = UserRole.GeneralManager,
            TeamId = 1,
            AssignedAt = DateTime.UtcNow
        });

        _mockUserRepository
            .Setup(repo => repo.GetByIdWithRolesAsync(1))
            .ReturnsAsync(user);
        _mockAuthorizationService
            .Setup(s => s.IsGlobalAdminAsync("test-oid-123"))
            .ReturnsAsync(true);
        _mockUserRepository
            .Setup(repo => repo.UpdateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.RemoveLeagueRole(1, 1);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        _mockUserRepository.Verify(
            repo => repo.UpdateAsync(It.Is<User>(u =>
            u.LeagueRoles.First(lr => lr.Id == 1).IsDeleted == true)), Times.Once);
    }

    [Fact]
    public async Task RemoveLeagueRole_AsCommissioner_SuccessfullySoftDeletesRole()
    {
        // Arrange
        var user = CreateTestUser(1, "oid-1", "user1@example.com", "User 1");
        user.LeagueRoles.Add(new UserLeagueRole
        {
            Id = 1,
            UserId = 1,
            LeagueId = 1,
            Role = UserRole.GeneralManager,
            TeamId = 1,
            AssignedAt = DateTime.UtcNow
        });

        _mockUserRepository
            .Setup(repo => repo.GetByIdWithRolesAsync(1))
            .ReturnsAsync(user);
        _mockAuthorizationService
            .Setup(s => s.IsGlobalAdminAsync("test-oid-123"))
            .ReturnsAsync(false);
        _mockAuthorizationService
            .Setup(s => s.IsCommissionerOfLeagueAsync("test-oid-123", 1))
            .ReturnsAsync(true);
        _mockUserRepository
            .Setup(repo => repo.UpdateAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.RemoveLeagueRole(1, 1);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        _mockUserRepository.Verify(
            repo => repo.UpdateAsync(It.Is<User>(u =>
            u.LeagueRoles.First(lr => lr.Id == 1).IsDeleted == true)), Times.Once);
    }

    #endregion
}

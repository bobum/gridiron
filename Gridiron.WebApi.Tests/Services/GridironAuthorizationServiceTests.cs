using DataAccessLayer.Repositories;
using DomainObjects;
using FluentAssertions;
using Gridiron.WebApi.Services;
using Moq;
using Xunit;

namespace Gridiron.WebApi.Tests.Services;

/// <summary>
/// Unit tests for GridironAuthorizationService
/// CRITICAL SECURITY: These tests verify authorization boundaries
/// Tests use mocked repositories to isolate authorization logic
/// </summary>
public class GridironAuthorizationServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly Mock<ITeamRepository> _mockTeamRepo;
    private readonly Mock<IDivisionRepository> _mockDivisionRepo;
    private readonly Mock<IConferenceRepository> _mockConferenceRepo;
    private readonly GridironAuthorizationService _service;

    public GridironAuthorizationServiceTests()
    {
        _mockUserRepo = new Mock<IUserRepository>();
        _mockTeamRepo = new Mock<ITeamRepository>();
        _mockDivisionRepo = new Mock<IDivisionRepository>();
        _mockConferenceRepo = new Mock<IConferenceRepository>();
        _service = new GridironAuthorizationService(
            _mockUserRepo.Object,
            _mockTeamRepo.Object,
            _mockDivisionRepo.Object,
            _mockConferenceRepo.Object);
    }

    #region GetOrCreateUserFromClaimsAsync Tests

    [Fact]
    public async Task GetOrCreateUserFromClaimsAsync_WithNewUser_CreatesUser()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.GetByAzureAdObjectIdAsync("new-oid"))
            .ReturnsAsync((User?)null);
        _mockUserRepo.Setup(r => r.AddAsync(It.IsAny<User>()))
            .ReturnsAsync((User u) => { u.Id = 1; return u; });

        // Act
        var result = await _service.GetOrCreateUserFromClaimsAsync("new-oid", "new@example.com", "New User");

        // Assert
        result.Should().NotBeNull();
        result.AzureAdObjectId.Should().Be("new-oid");
        result.Email.Should().Be("new@example.com");
        result.DisplayName.Should().Be("New User");
        result.IsGlobalAdmin.Should().BeFalse();
        _mockUserRepo.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task GetOrCreateUserFromClaimsAsync_WithExistingUser_UpdatesLastLogin()
    {
        // Arrange
        var existingUser = new User
        {
            Id = 1,
            AzureAdObjectId = "existing-oid",
            Email = "existing@example.com",
            DisplayName = "Existing User",
            LastLoginAt = DateTime.UtcNow.AddDays(-1)
        };
        _mockUserRepo.Setup(r => r.GetByAzureAdObjectIdAsync("existing-oid"))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _service.GetOrCreateUserFromClaimsAsync("existing-oid", "existing@example.com", "Existing User");

        // Assert
        result.Should().NotBeNull();
        result.LastLoginAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        _mockUserRepo.Verify(r => r.UpdateAsync(existingUser), Times.Once);
        _mockUserRepo.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
    }

    #endregion

    #region IsGlobalAdminAsync Tests

    [Fact]
    public async Task IsGlobalAdminAsync_WithGlobalAdmin_ReturnsTrue()
    {
        // Arrange
        var admin = new User { Id = 1, AzureAdObjectId = "admin-oid", IsGlobalAdmin = true };
        _mockUserRepo.Setup(r => r.GetByAzureAdObjectIdAsync("admin-oid"))
            .ReturnsAsync(admin);

        // Act
        var result = await _service.IsGlobalAdminAsync("admin-oid");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsGlobalAdminAsync_WithRegularUser_ReturnsFalse()
    {
        // Arrange
        var user = new User { Id = 1, AzureAdObjectId = "user-oid", IsGlobalAdmin = false };
        _mockUserRepo.Setup(r => r.GetByAzureAdObjectIdAsync("user-oid"))
            .ReturnsAsync(user);

        // Act
        var result = await _service.IsGlobalAdminAsync("user-oid");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsGlobalAdminAsync_WithNonExistentUser_ReturnsFalse()
    {
        // Arrange
        _mockUserRepo.Setup(r => r.GetByAzureAdObjectIdAsync("nonexistent-oid"))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _service.IsGlobalAdminAsync("nonexistent-oid");

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region CanAccessLeagueAsync Tests

    [Fact]
    public async Task CanAccessLeagueAsync_WithGlobalAdmin_ReturnsTrue()
    {
        // Arrange
        var admin = new User { Id = 1, AzureAdObjectId = "admin-oid", IsGlobalAdmin = true };
        _mockUserRepo.Setup(r => r.GetByAzureAdObjectIdAsync("admin-oid"))
            .ReturnsAsync(admin);

        // Act
        var result = await _service.CanAccessLeagueAsync("admin-oid", 1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanAccessLeagueAsync_WithCommissioner_ReturnsTrue()
    {
        // Arrange
        var admin = new User { Id = 1, AzureAdObjectId = "user-oid", IsGlobalAdmin = false };
        _mockUserRepo.Setup(r => r.GetByAzureAdObjectIdAsync("user-oid"))
            .ReturnsAsync(admin);

        var user = new User
        {
            Id = 1,
            AzureAdObjectId = "user-oid",
            LeagueRoles = new List<UserLeagueRole>
            {
                new UserLeagueRole { LeagueId = 1, Role = UserRole.Commissioner }
            }
        };
        _mockUserRepo.Setup(r => r.GetByAzureAdObjectIdWithRolesAsync("user-oid"))
            .ReturnsAsync(user);

        // Act
        var result = await _service.CanAccessLeagueAsync("user-oid", 1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanAccessLeagueAsync_WithGMOfTeamInLeague_ReturnsTrue()
    {
        // Arrange
        var admin = new User { Id = 1, AzureAdObjectId = "gm-oid", IsGlobalAdmin = false };
        _mockUserRepo.Setup(r => r.GetByAzureAdObjectIdAsync("gm-oid"))
            .ReturnsAsync(admin);

        var user = new User
        {
            Id = 1,
            AzureAdObjectId = "gm-oid",
            LeagueRoles = new List<UserLeagueRole>
            {
                new UserLeagueRole { LeagueId = 1, TeamId = 5, Role = UserRole.GeneralManager }
            }
        };
        _mockUserRepo.Setup(r => r.GetByAzureAdObjectIdWithRolesAsync("gm-oid"))
            .ReturnsAsync(user);

        // Act
        var result = await _service.CanAccessLeagueAsync("gm-oid", 1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanAccessLeagueAsync_WithNoRoleInLeague_ReturnsFalse()
    {
        // Arrange
        var admin = new User { Id = 1, AzureAdObjectId = "user-oid", IsGlobalAdmin = false };
        _mockUserRepo.Setup(r => r.GetByAzureAdObjectIdAsync("user-oid"))
            .ReturnsAsync(admin);

        var user = new User
        {
            Id = 1,
            AzureAdObjectId = "user-oid",
            LeagueRoles = new List<UserLeagueRole>
            {
                new UserLeagueRole { LeagueId = 2, Role = UserRole.Commissioner } // Different league
            }
        };
        _mockUserRepo.Setup(r => r.GetByAzureAdObjectIdWithRolesAsync("user-oid"))
            .ReturnsAsync(user);

        // Act
        var result = await _service.CanAccessLeagueAsync("user-oid", 1);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region CanAccessTeamAsync Tests

    [Fact]
    public async Task CanAccessTeamAsync_WithGlobalAdmin_ReturnsTrue()
    {
        // Arrange
        var admin = new User { Id = 1, AzureAdObjectId = "admin-oid", IsGlobalAdmin = true };
        _mockUserRepo.Setup(r => r.GetByAzureAdObjectIdAsync("admin-oid"))
            .ReturnsAsync(admin);

        // Act
        var result = await _service.CanAccessTeamAsync("admin-oid", 1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanAccessTeamAsync_WithCommissionerOfLeague_ReturnsTrue()
    {
        // Arrange
        SetupTeamHierarchy(teamId: 1, divisionId: 10, conferenceId: 20, leagueId: 100);

        var admin = new User { Id = 1, AzureAdObjectId = "commissioner-oid", IsGlobalAdmin = false };
        _mockUserRepo.Setup(r => r.GetByAzureAdObjectIdAsync("commissioner-oid"))
            .ReturnsAsync(admin);

        var user = new User
        {
            Id = 1,
            AzureAdObjectId = "commissioner-oid",
            LeagueRoles = new List<UserLeagueRole>
            {
                new UserLeagueRole { LeagueId = 100, Role = UserRole.Commissioner }
            }
        };
        _mockUserRepo.Setup(r => r.GetByAzureAdObjectIdWithRolesAsync("commissioner-oid"))
            .ReturnsAsync(user);

        // Act
        var result = await _service.CanAccessTeamAsync("commissioner-oid", 1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanAccessTeamAsync_WithGMOfTeam_ReturnsTrue()
    {
        // Arrange
        SetupTeamHierarchy(teamId: 1, divisionId: 10, conferenceId: 20, leagueId: 100);

        var admin = new User { Id = 1, AzureAdObjectId = "gm-oid", IsGlobalAdmin = false };
        _mockUserRepo.Setup(r => r.GetByAzureAdObjectIdAsync("gm-oid"))
            .ReturnsAsync(admin);

        var user = new User
        {
            Id = 1,
            AzureAdObjectId = "gm-oid",
            LeagueRoles = new List<UserLeagueRole>
            {
                new UserLeagueRole { LeagueId = 100, TeamId = 1, Role = UserRole.GeneralManager }
            }
        };
        _mockUserRepo.Setup(r => r.GetByAzureAdObjectIdWithRolesAsync("gm-oid"))
            .ReturnsAsync(user);

        // Act
        var result = await _service.CanAccessTeamAsync("gm-oid", 1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task CanAccessTeamAsync_WithGMOfDifferentTeam_ReturnsFalse()
    {
        // Arrange
        SetupTeamHierarchy(teamId: 1, divisionId: 10, conferenceId: 20, leagueId: 100);

        var admin = new User { Id = 1, AzureAdObjectId = "gm-oid", IsGlobalAdmin = false };
        _mockUserRepo.Setup(r => r.GetByAzureAdObjectIdAsync("gm-oid"))
            .ReturnsAsync(admin);

        var user = new User
        {
            Id = 1,
            AzureAdObjectId = "gm-oid",
            LeagueRoles = new List<UserLeagueRole>
            {
                new UserLeagueRole { LeagueId = 100, TeamId = 2, Role = UserRole.GeneralManager } // Different team
            }
        };
        _mockUserRepo.Setup(r => r.GetByAzureAdObjectIdWithRolesAsync("gm-oid"))
            .ReturnsAsync(user);

        // Act
        var result = await _service.CanAccessTeamAsync("gm-oid", 1);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CanAccessTeamAsync_WithCommissionerOfDifferentLeague_ReturnsFalse()
    {
        // Arrange
        SetupTeamHierarchy(teamId: 1, divisionId: 10, conferenceId: 20, leagueId: 100);

        var admin = new User { Id = 1, AzureAdObjectId = "commissioner-oid", IsGlobalAdmin = false };
        _mockUserRepo.Setup(r => r.GetByAzureAdObjectIdAsync("commissioner-oid"))
            .ReturnsAsync(admin);

        var user = new User
        {
            Id = 1,
            AzureAdObjectId = "commissioner-oid",
            LeagueRoles = new List<UserLeagueRole>
            {
                new UserLeagueRole { LeagueId = 200, Role = UserRole.Commissioner } // Different league
            }
        };
        _mockUserRepo.Setup(r => r.GetByAzureAdObjectIdWithRolesAsync("commissioner-oid"))
            .ReturnsAsync(user);

        // Act
        var result = await _service.CanAccessTeamAsync("commissioner-oid", 1);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region IsCommissionerOfLeagueAsync Tests

    [Fact]
    public async Task IsCommissionerOfLeagueAsync_WithGlobalAdmin_ReturnsTrue()
    {
        // Arrange
        var admin = new User { Id = 1, AzureAdObjectId = "admin-oid", IsGlobalAdmin = true };
        _mockUserRepo.Setup(r => r.GetByAzureAdObjectIdAsync("admin-oid"))
            .ReturnsAsync(admin);

        // Act
        var result = await _service.IsCommissionerOfLeagueAsync("admin-oid", 1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsCommissionerOfLeagueAsync_WithCommissioner_ReturnsTrue()
    {
        // Arrange
        var admin = new User { Id = 1, AzureAdObjectId = "commissioner-oid", IsGlobalAdmin = false };
        _mockUserRepo.Setup(r => r.GetByAzureAdObjectIdAsync("commissioner-oid"))
            .ReturnsAsync(admin);

        var user = new User
        {
            Id = 1,
            AzureAdObjectId = "commissioner-oid",
            LeagueRoles = new List<UserLeagueRole>
            {
                new UserLeagueRole { LeagueId = 1, Role = UserRole.Commissioner }
            }
        };
        _mockUserRepo.Setup(r => r.GetByAzureAdObjectIdWithRolesAsync("commissioner-oid"))
            .ReturnsAsync(user);

        // Act
        var result = await _service.IsCommissionerOfLeagueAsync("commissioner-oid", 1);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsCommissionerOfLeagueAsync_WithGM_ReturnsFalse()
    {
        // Arrange
        var admin = new User { Id = 1, AzureAdObjectId = "gm-oid", IsGlobalAdmin = false };
        _mockUserRepo.Setup(r => r.GetByAzureAdObjectIdAsync("gm-oid"))
            .ReturnsAsync(admin);

        var user = new User
        {
            Id = 1,
            AzureAdObjectId = "gm-oid",
            LeagueRoles = new List<UserLeagueRole>
            {
                new UserLeagueRole { LeagueId = 1, TeamId = 5, Role = UserRole.GeneralManager }
            }
        };
        _mockUserRepo.Setup(r => r.GetByAzureAdObjectIdWithRolesAsync("gm-oid"))
            .ReturnsAsync(user);

        // Act
        var result = await _service.IsCommissionerOfLeagueAsync("gm-oid", 1);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region IsGeneralManagerOfTeamAsync Tests

    [Fact]
    public async Task IsGeneralManagerOfTeamAsync_WithGMOfTeam_ReturnsTrue()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            AzureAdObjectId = "gm-oid",
            LeagueRoles = new List<UserLeagueRole>
            {
                new UserLeagueRole { LeagueId = 1, TeamId = 5, Role = UserRole.GeneralManager }
            }
        };
        _mockUserRepo.Setup(r => r.GetByAzureAdObjectIdWithRolesAsync("gm-oid"))
            .ReturnsAsync(user);

        // Act
        var result = await _service.IsGeneralManagerOfTeamAsync("gm-oid", 5);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsGeneralManagerOfTeamAsync_WithGMOfDifferentTeam_ReturnsFalse()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            AzureAdObjectId = "gm-oid",
            LeagueRoles = new List<UserLeagueRole>
            {
                new UserLeagueRole { LeagueId = 1, TeamId = 5, Role = UserRole.GeneralManager }
            }
        };
        _mockUserRepo.Setup(r => r.GetByAzureAdObjectIdWithRolesAsync("gm-oid"))
            .ReturnsAsync(user);

        // Act
        var result = await _service.IsGeneralManagerOfTeamAsync("gm-oid", 6); // Different team

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsGeneralManagerOfTeamAsync_WithCommissioner_ReturnsFalse()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            AzureAdObjectId = "commissioner-oid",
            LeagueRoles = new List<UserLeagueRole>
            {
                new UserLeagueRole { LeagueId = 1, Role = UserRole.Commissioner }
            }
        };
        _mockUserRepo.Setup(r => r.GetByAzureAdObjectIdWithRolesAsync("commissioner-oid"))
            .ReturnsAsync(user);

        // Act
        var result = await _service.IsGeneralManagerOfTeamAsync("commissioner-oid", 5);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Helper Methods

    private void SetupTeamHierarchy(int teamId, int divisionId, int conferenceId, int leagueId)
    {
        var team = new Team { Id = teamId, DivisionId = divisionId, Name = "Test Team" };
        var division = new Division { Id = divisionId, ConferenceId = conferenceId, Name = "Test Division" };
        var conference = new Conference { Id = conferenceId, LeagueId = leagueId, Name = "Test Conference" };

        _mockTeamRepo.Setup(r => r.GetByIdAsync(teamId)).ReturnsAsync(team);
        _mockDivisionRepo.Setup(r => r.GetByIdAsync(divisionId)).ReturnsAsync(division);
        _mockConferenceRepo.Setup(r => r.GetByIdAsync(conferenceId)).ReturnsAsync(conference);
    }

    #endregion
}

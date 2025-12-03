using DataAccessLayer.Repositories;
using DomainObjects;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Gridiron.IntegrationTests;

/// <summary>
/// Integration tests for UserRepository
/// Tests the full stack: Repository → Database
/// Uses SQLite in-memory database to properly test EF Core behavior
/// CRITICAL: These tests verify authorization security boundaries.
/// </summary>
public class UserRepositoryTests : IClassFixture<DatabaseTestFixture>
{
    private readonly DatabaseTestFixture _fixture;

    public UserRepositoryTests(DatabaseTestFixture fixture)
    {
        _fixture = fixture;
    }

    #region GetByAzureAdObjectIdAsync Tests

    [Fact]
    public async Task GetByAzureAdObjectIdAsync_WithExistingUser_ReturnsUser()
    {
        // Arrange
        var userRepo = _fixture.ServiceProvider.GetRequiredService<IUserRepository>();
        var user = new User
        {
            AzureAdObjectId = "test-oid-123",
            Email = "test@example.com",
            DisplayName = "Test User",
            IsGlobalAdmin = false
        };
        await userRepo.AddAsync(user);

        // Act
        var result = await userRepo.GetByAzureAdObjectIdAsync("test-oid-123");

        // Assert
        result.Should().NotBeNull();
        result!.AzureAdObjectId.Should().Be("test-oid-123");
        result.Email.Should().Be("test@example.com");
        result.DisplayName.Should().Be("Test User");
        result.IsGlobalAdmin.Should().BeFalse();
    }

    [Fact]
    public async Task GetByAzureAdObjectIdAsync_WithNonExistentUser_ReturnsNull()
    {
        // Arrange
        var userRepo = _fixture.ServiceProvider.GetRequiredService<IUserRepository>();

        // Act
        var result = await userRepo.GetByAzureAdObjectIdAsync("nonexistent-oid");

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithExistingUser_ReturnsUser()
    {
        // Arrange
        var userRepo = _fixture.ServiceProvider.GetRequiredService<IUserRepository>();
        var user = new User
        {
            AzureAdObjectId = "test-oid-456",
            Email = "user@example.com",
            DisplayName = "Another User",
            IsGlobalAdmin = false
        };
        await userRepo.AddAsync(user);

        // Act
        var result = await userRepo.GetByIdAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.AzureAdObjectId.Should().Be("test-oid-456");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        var userRepo = _fixture.ServiceProvider.GetRequiredService<IUserRepository>();

        // Act
        var result = await userRepo.GetByIdAsync(99999);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetByIdWithRolesAsync Tests

    [Fact]
    public async Task GetByIdWithRolesAsync_WithUserAndRoles_ReturnsUserWithRoles()
    {
        // Arrange
        var userRepo = _fixture.ServiceProvider.GetRequiredService<IUserRepository>();
        var leagueRepo = _fixture.ServiceProvider.GetRequiredService<ILeagueRepository>();
        var teamRepo = _fixture.ServiceProvider.GetRequiredService<ITeamRepository>();

        var user = new User
        {
            AzureAdObjectId = "test-oid-roles",
            Email = "roles@example.com",
            DisplayName = "Role User",
            IsGlobalAdmin = false
        };
        await userRepo.AddAsync(user);

        var league = new League { Name = "Test League", Season = 2024, IsActive = true };
        await leagueRepo.AddAsync(league);

        var team = new Team { Name = "Test Team", City = "Test City" };
        await teamRepo.AddAsync(team);

        user.LeagueRoles.Add(new UserLeagueRole
        {
            UserId = user.Id,
            LeagueId = league.Id,
            Role = UserRole.Commissioner,
            TeamId = null
        });

        user.LeagueRoles.Add(new UserLeagueRole
        {
            UserId = user.Id,
            LeagueId = league.Id,
            Role = UserRole.GeneralManager,
            TeamId = team.Id
        });

        await userRepo.UpdateAsync(user);

        // Act
        var result = await userRepo.GetByIdWithRolesAsync(user.Id);

        // Assert
        result.Should().NotBeNull();
        result!.LeagueRoles.Should().HaveCount(2);
        result.LeagueRoles.Should().Contain(ulr => ulr.Role == UserRole.Commissioner && ulr.TeamId == null);
        result.LeagueRoles.Should().Contain(ulr => ulr.Role == UserRole.GeneralManager && ulr.TeamId == team.Id);
    }

    #endregion

    #region GetByAzureAdObjectIdWithRolesAsync Tests

    [Fact]
    public async Task GetByAzureAdObjectIdWithRolesAsync_WithUserAndRoles_ReturnsUserWithRoles()
    {
        // Arrange
        var userRepo = _fixture.ServiceProvider.GetRequiredService<IUserRepository>();
        var leagueRepo = _fixture.ServiceProvider.GetRequiredService<ILeagueRepository>();

        var user = new User
        {
            AzureAdObjectId = "test-oid-azure-roles",
            Email = "azureroles@example.com",
            DisplayName = "Azure Role User",
            IsGlobalAdmin = false
        };
        await userRepo.AddAsync(user);

        var league = new League { Name = "Azure Test League", Season = 2024, IsActive = true };
        await leagueRepo.AddAsync(league);

        user.LeagueRoles.Add(new UserLeagueRole
        {
            UserId = user.Id,
            LeagueId = league.Id,
            Role = UserRole.Commissioner
        });

        await userRepo.UpdateAsync(user);

        // Act
        var result = await userRepo.GetByAzureAdObjectIdWithRolesAsync("test-oid-azure-roles");

        // Assert
        result.Should().NotBeNull();
        result!.LeagueRoles.Should().HaveCount(1);
        result.LeagueRoles[0].Role.Should().Be(UserRole.Commissioner);
        result.LeagueRoles[0].LeagueId.Should().Be(league.Id);
    }

    #endregion

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_WithValidUser_AddsUserToDatabase()
    {
        // Arrange
        var userRepo = _fixture.ServiceProvider.GetRequiredService<IUserRepository>();
        var user = new User
        {
            AzureAdObjectId = "new-user-oid",
            Email = "newuser@example.com",
            DisplayName = "New User",
            IsGlobalAdmin = false,
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow
        };

        // Act
        var result = await userRepo.AddAsync(user);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);

        var retrieved = await userRepo.GetByIdAsync(result.Id);
        retrieved.Should().NotBeNull();
        retrieved!.AzureAdObjectId.Should().Be("new-user-oid");
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithModifiedUser_UpdatesDatabase()
    {
        // Arrange
        var userRepo = _fixture.ServiceProvider.GetRequiredService<IUserRepository>();
        var user = new User
        {
            AzureAdObjectId = "update-test-oid",
            Email = "update@example.com",
            DisplayName = "Original Name",
            IsGlobalAdmin = false
        };
        await userRepo.AddAsync(user);

        // Act
        user.DisplayName = "Updated Name";
        user.LastLoginAt = DateTime.UtcNow;
        await userRepo.UpdateAsync(user);

        // Assert
        var updated = await userRepo.GetByIdAsync(user.Id);
        updated.Should().NotBeNull();
        updated!.DisplayName.Should().Be("Updated Name");
    }

    #endregion

    #region GetGlobalAdminsAsync Tests

    [Fact]
    public async Task GetGlobalAdminsAsync_WithMultipleUsers_ReturnsOnlyAdmins()
    {
        // Arrange
        var userRepo = _fixture.ServiceProvider.GetRequiredService<IUserRepository>();

        var admin1 = new User
        {
            AzureAdObjectId = "admin-1",
            Email = "admin1@example.com",
            DisplayName = "Admin One",
            IsGlobalAdmin = true
        };
        await userRepo.AddAsync(admin1);

        var admin2 = new User
        {
            AzureAdObjectId = "admin-2",
            Email = "admin2@example.com",
            DisplayName = "Admin Two",
            IsGlobalAdmin = true
        };
        await userRepo.AddAsync(admin2);

        var regularUser = new User
        {
            AzureAdObjectId = "regular-user",
            Email = "regular@example.com",
            DisplayName = "Regular User",
            IsGlobalAdmin = false
        };
        await userRepo.AddAsync(regularUser);

        // Act
        var admins = await userRepo.GetGlobalAdminsAsync();

        // Assert
        admins.Should().HaveCount(2);
        admins.Should().AllSatisfy(u => u.IsGlobalAdmin.Should().BeTrue());
        admins.Should().Contain(u => u.AzureAdObjectId == "admin-1");
        admins.Should().Contain(u => u.AzureAdObjectId == "admin-2");
        admins.Should().NotContain(u => u.AzureAdObjectId == "regular-user");
    }

    #endregion

    #region ExistsAsync Tests

    [Fact]
    public async Task ExistsAsync_WithExistingUser_ReturnsTrue()
    {
        // Arrange
        var userRepo = _fixture.ServiceProvider.GetRequiredService<IUserRepository>();
        var user = new User
        {
            AzureAdObjectId = "exists-test-oid",
            Email = "exists@example.com",
            DisplayName = "Exists User",
            IsGlobalAdmin = false
        };
        await userRepo.AddAsync(user);

        // Act
        var exists = await userRepo.ExistsAsync("exists-test-oid");

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistentUser_ReturnsFalse()
    {
        // Arrange
        var userRepo = _fixture.ServiceProvider.GetRequiredService<IUserRepository>();

        // Act
        var exists = await userRepo.ExistsAsync("nonexistent-oid");

        // Assert
        exists.Should().BeFalse();
    }

    #endregion
}

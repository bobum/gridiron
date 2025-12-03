using DataAccessLayer.Repositories;
using DomainObjects;
using FluentAssertions;
using GameManagement.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Gridiron.IntegrationTests;

/// <summary>
/// Integration tests for UPDATE endpoints (PUT operations)
/// Tests the full stack: Controller logic simulation → Service → Repository → Database
/// Uses SQLite in-memory database to properly test EF Core behavior.
/// </summary>
public class UpdateEndpointsIntegrationTests : IClassFixture<DatabaseTestFixture>
{
    private readonly DatabaseTestFixture _fixture;

    public UpdateEndpointsIntegrationTests(DatabaseTestFixture fixture)
    {
        _fixture = fixture;
    }

    // ==========================================
    // LEAGUE UPDATE ENDPOINT TESTS
    // ==========================================
    [Fact]
    public async Task UpdateLeague_WithValidName_UpdatesNameInDatabase()
    {
        // Arrange
        var leagueRepo = _fixture.ServiceProvider.GetRequiredService<ILeagueRepository>();
        var leagueBuilder = _fixture.ServiceProvider.GetRequiredService<ILeagueBuilderService>();

        var league = new League { Name = "Original League", Season = 2024, IsActive = true };
        await leagueRepo.AddAsync(league);
        var leagueId = league.Id;

        // Act - Simulate controller behavior
        var fetchedLeague = await leagueRepo.GetByIdAsync(leagueId);
        fetchedLeague.Should().NotBeNull();

        leagueBuilder.UpdateLeague(fetchedLeague!, "Updated League Name", null, null);
        await leagueRepo.UpdateAsync(fetchedLeague!);

        // Assert - Verify in database
        var updatedLeague = await leagueRepo.GetByIdAsync(leagueId);
        updatedLeague.Should().NotBeNull();
        updatedLeague!.Name.Should().Be("Updated League Name");
        updatedLeague.Season.Should().Be(2024); // Unchanged
        updatedLeague.IsActive.Should().BeTrue(); // Unchanged
    }

    [Fact]
    public async Task UpdateLeague_WithValidSeason_UpdatesSeasonInDatabase()
    {
        // Arrange
        var leagueRepo = _fixture.ServiceProvider.GetRequiredService<ILeagueRepository>();
        var leagueBuilder = _fixture.ServiceProvider.GetRequiredService<ILeagueBuilderService>();

        var league = new League { Name = "Test League", Season = 2024, IsActive = true };
        await leagueRepo.AddAsync(league);
        var leagueId = league.Id;

        // Act
        var fetchedLeague = await leagueRepo.GetByIdAsync(leagueId);
        leagueBuilder.UpdateLeague(fetchedLeague!, null, 2025, null);
        await leagueRepo.UpdateAsync(fetchedLeague!);

        // Assert
        var updatedLeague = await leagueRepo.GetByIdAsync(leagueId);
        updatedLeague.Should().NotBeNull();
        updatedLeague!.Season.Should().Be(2025);
        updatedLeague.Name.Should().Be("Test League"); // Unchanged
    }

    [Fact]
    public async Task UpdateLeague_WithValidIsActive_UpdatesIsActiveInDatabase()
    {
        // Arrange
        var leagueRepo = _fixture.ServiceProvider.GetRequiredService<ILeagueRepository>();
        var leagueBuilder = _fixture.ServiceProvider.GetRequiredService<ILeagueBuilderService>();

        var league = new League { Name = "Test League", Season = 2024, IsActive = true };
        await leagueRepo.AddAsync(league);
        var leagueId = league.Id;

        // Act
        var fetchedLeague = await leagueRepo.GetByIdAsync(leagueId);
        leagueBuilder.UpdateLeague(fetchedLeague!, null, null, false);
        await leagueRepo.UpdateAsync(fetchedLeague!);

        // Assert
        var updatedLeague = await leagueRepo.GetByIdAsync(leagueId);
        updatedLeague.Should().NotBeNull();
        updatedLeague!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateLeague_WithAllParameters_UpdatesAllFieldsInDatabase()
    {
        // Arrange
        var leagueRepo = _fixture.ServiceProvider.GetRequiredService<ILeagueRepository>();
        var leagueBuilder = _fixture.ServiceProvider.GetRequiredService<ILeagueBuilderService>();

        var league = new League { Name = "Original", Season = 2023, IsActive = false };
        await leagueRepo.AddAsync(league);
        var leagueId = league.Id;

        // Act
        var fetchedLeague = await leagueRepo.GetByIdAsync(leagueId);
        leagueBuilder.UpdateLeague(fetchedLeague!, "Completely Updated", 2026, true);
        await leagueRepo.UpdateAsync(fetchedLeague!);

        // Assert
        var updatedLeague = await leagueRepo.GetByIdAsync(leagueId);
        updatedLeague.Should().NotBeNull();
        updatedLeague!.Name.Should().Be("Completely Updated");
        updatedLeague.Season.Should().Be(2026);
        updatedLeague.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateLeague_SoftDeletedEntity_ReturnsNull()
    {
        // Arrange
        var leagueRepo = _fixture.ServiceProvider.GetRequiredService<ILeagueRepository>();

        var league = new League { Name = "Deleted League", Season = 2024, IsActive = true };
        await leagueRepo.AddAsync(league);
        var leagueId = league.Id;

        await leagueRepo.SoftDeleteAsync(leagueId, "TestUser", "Testing");

        // Act - Try to fetch soft-deleted league (should return null due to query filters)
        var fetchedLeague = await leagueRepo.GetByIdAsync(leagueId);

        // Assert
        fetchedLeague.Should().BeNull("because soft-deleted entities are excluded from normal queries");
    }

    // ==========================================
    // CONFERENCE UPDATE ENDPOINT TESTS
    // ==========================================
    [Fact]
    public async Task UpdateConference_WithValidName_UpdatesNameInDatabase()
    {
        // Arrange
        var leagueRepo = _fixture.ServiceProvider.GetRequiredService<ILeagueRepository>();
        var conferenceRepo = _fixture.ServiceProvider.GetRequiredService<IConferenceRepository>();
        var conferenceBuilder = _fixture.ServiceProvider.GetRequiredService<IConferenceBuilderService>();
        var leagueBuilder = _fixture.ServiceProvider.GetRequiredService<ILeagueBuilderService>();

        // Create league with 1 conference, 1 division, 1 team (minimum valid structure)
        var league = leagueBuilder.CreateLeague("Test League", 1, 1, 1);
        await leagueRepo.AddAsync(league);
        var conference = league.Conferences.First();
        var conferenceId = conference.Id;

        // Act
        var fetchedConference = await conferenceRepo.GetByIdAsync(conferenceId);
        fetchedConference.Should().NotBeNull();

        conferenceBuilder.UpdateConference(fetchedConference!, "Updated Conference");
        await conferenceRepo.UpdateAsync(fetchedConference!);

        // Assert
        var updatedConference = await conferenceRepo.GetByIdAsync(conferenceId);
        updatedConference.Should().NotBeNull();
        updatedConference!.Name.Should().Be("Updated Conference");
    }

    [Fact]
    public async Task UpdateConference_SoftDeletedEntity_ReturnsNull()
    {
        // Arrange
        var leagueRepo = _fixture.ServiceProvider.GetRequiredService<ILeagueRepository>();
        var conferenceRepo = _fixture.ServiceProvider.GetRequiredService<IConferenceRepository>();
        var leagueBuilder = _fixture.ServiceProvider.GetRequiredService<ILeagueBuilderService>();

        var league = leagueBuilder.CreateLeague("Test League", 1, 1, 1);
        await leagueRepo.AddAsync(league);
        var conferenceId = league.Conferences.First().Id;

        await conferenceRepo.SoftDeleteAsync(conferenceId, "TestUser", "Testing");

        // Act
        var fetchedConference = await conferenceRepo.GetByIdAsync(conferenceId);

        // Assert
        fetchedConference.Should().BeNull("because soft-deleted entities are excluded from normal queries");
    }

    // ==========================================
    // DIVISION UPDATE ENDPOINT TESTS
    // ==========================================
    [Fact]
    public async Task UpdateDivision_WithValidName_UpdatesNameInDatabase()
    {
        // Arrange
        var leagueRepo = _fixture.ServiceProvider.GetRequiredService<ILeagueRepository>();
        var divisionRepo = _fixture.ServiceProvider.GetRequiredService<IDivisionRepository>();
        var divisionBuilder = _fixture.ServiceProvider.GetRequiredService<IDivisionBuilderService>();
        var leagueBuilder = _fixture.ServiceProvider.GetRequiredService<ILeagueBuilderService>();

        // Create league with 1 conference → 1 division → 1 team (minimum valid structure)
        var league = leagueBuilder.CreateLeague("Test League", 1, 1, 1);
        await leagueRepo.AddAsync(league);
        var division = league.Conferences.First().Divisions.First();
        var divisionId = division.Id;

        // Act
        var fetchedDivision = await divisionRepo.GetByIdAsync(divisionId);
        fetchedDivision.Should().NotBeNull();

        divisionBuilder.UpdateDivision(fetchedDivision!, "Updated Division");
        await divisionRepo.UpdateAsync(fetchedDivision!);

        // Assert
        var updatedDivision = await divisionRepo.GetByIdAsync(divisionId);
        updatedDivision.Should().NotBeNull();
        updatedDivision!.Name.Should().Be("Updated Division");
    }

    [Fact]
    public async Task UpdateDivision_SoftDeletedEntity_ReturnsNull()
    {
        // Arrange
        var leagueRepo = _fixture.ServiceProvider.GetRequiredService<ILeagueRepository>();
        var divisionRepo = _fixture.ServiceProvider.GetRequiredService<IDivisionRepository>();
        var leagueBuilder = _fixture.ServiceProvider.GetRequiredService<ILeagueBuilderService>();

        var league = leagueBuilder.CreateLeague("Test League", 1, 1, 1);
        await leagueRepo.AddAsync(league);
        var divisionId = league.Conferences.First().Divisions.First().Id;

        await divisionRepo.SoftDeleteAsync(divisionId, "TestUser", "Testing");

        // Act
        var fetchedDivision = await divisionRepo.GetByIdAsync(divisionId);

        // Assert
        fetchedDivision.Should().BeNull("because soft-deleted entities are excluded from normal queries");
    }

    // ==========================================
    // TEAM UPDATE ENDPOINT TESTS
    // ==========================================
    [Fact]
    public async Task UpdateTeam_WithValidName_UpdatesNameInDatabase()
    {
        // Arrange
        var leagueRepo = _fixture.ServiceProvider.GetRequiredService<ILeagueRepository>();
        var teamRepo = _fixture.ServiceProvider.GetRequiredService<ITeamRepository>();
        var teamBuilder = _fixture.ServiceProvider.GetRequiredService<ITeamBuilderService>();
        var leagueBuilder = _fixture.ServiceProvider.GetRequiredService<ILeagueBuilderService>();

        // Create league with 1 conference → 1 division → 1 team
        var league = leagueBuilder.CreateLeague("Test League", 1, 1, 1);
        await leagueRepo.AddAsync(league);
        var team = league.Conferences.First().Divisions.First().Teams.First();
        var teamId = team.Id;
        var originalName = team.Name;

        // Act
        var fetchedTeam = await teamRepo.GetByIdAsync(teamId);
        fetchedTeam.Should().NotBeNull();

        teamBuilder.UpdateTeam(fetchedTeam!, "Updated Team Name", null, null, null, null, null, null, null, null);
        await teamRepo.UpdateAsync(fetchedTeam!);

        // Assert
        var updatedTeam = await teamRepo.GetByIdAsync(teamId);
        updatedTeam.Should().NotBeNull();
        updatedTeam!.Name.Should().Be("Updated Team Name");
        updatedTeam.Name.Should().NotBe(originalName);
    }

    [Fact]
    public async Task UpdateTeam_WithValidCity_UpdatesCityInDatabase()
    {
        // Arrange
        var leagueRepo = _fixture.ServiceProvider.GetRequiredService<ILeagueRepository>();
        var teamRepo = _fixture.ServiceProvider.GetRequiredService<ITeamRepository>();
        var teamBuilder = _fixture.ServiceProvider.GetRequiredService<ITeamBuilderService>();
        var leagueBuilder = _fixture.ServiceProvider.GetRequiredService<ILeagueBuilderService>();

        var league = leagueBuilder.CreateLeague("Test League", 1, 1, 1);
        await leagueRepo.AddAsync(league);
        var teamId = league.Conferences.First().Divisions.First().Teams.First().Id;

        // Act
        var fetchedTeam = await teamRepo.GetByIdAsync(teamId);
        teamBuilder.UpdateTeam(fetchedTeam!, null, "New City", null, null, null, null, null, null, null);
        await teamRepo.UpdateAsync(fetchedTeam!);

        // Assert
        var updatedTeam = await teamRepo.GetByIdAsync(teamId);
        updatedTeam.Should().NotBeNull();
        updatedTeam!.City.Should().Be("New City");
    }

    [Fact]
    public async Task UpdateTeam_WithValidBudget_UpdatesBudgetInDatabase()
    {
        // Arrange
        var leagueRepo = _fixture.ServiceProvider.GetRequiredService<ILeagueRepository>();
        var teamRepo = _fixture.ServiceProvider.GetRequiredService<ITeamRepository>();
        var teamBuilder = _fixture.ServiceProvider.GetRequiredService<ITeamBuilderService>();
        var leagueBuilder = _fixture.ServiceProvider.GetRequiredService<ILeagueBuilderService>();

        var league = leagueBuilder.CreateLeague("Test League", 1, 1, 1);
        await leagueRepo.AddAsync(league);
        var teamId = league.Conferences.First().Divisions.First().Teams.First().Id;

        // Act
        var fetchedTeam = await teamRepo.GetByIdAsync(teamId);
        teamBuilder.UpdateTeam(fetchedTeam!, null, null, 75000000, null, null, null, null, null, null);
        await teamRepo.UpdateAsync(fetchedTeam!);

        // Assert
        var updatedTeam = await teamRepo.GetByIdAsync(teamId);
        updatedTeam.Should().NotBeNull();
        updatedTeam!.Budget.Should().Be(75000000);
    }

    [Fact]
    public async Task UpdateTeam_WithValidStats_UpdatesStatsInDatabase()
    {
        // Arrange
        var leagueRepo = _fixture.ServiceProvider.GetRequiredService<ILeagueRepository>();
        var teamRepo = _fixture.ServiceProvider.GetRequiredService<ITeamRepository>();
        var teamBuilder = _fixture.ServiceProvider.GetRequiredService<ITeamBuilderService>();
        var leagueBuilder = _fixture.ServiceProvider.GetRequiredService<ILeagueBuilderService>();

        var league = leagueBuilder.CreateLeague("Test League", 1, 1, 1);
        await leagueRepo.AddAsync(league);
        var teamId = league.Conferences.First().Divisions.First().Teams.First().Id;

        // Act
        var fetchedTeam = await teamRepo.GetByIdAsync(teamId);
        teamBuilder.UpdateTeam(fetchedTeam!, null, null, null, 3, 10, 5, 2, null, null);
        await teamRepo.UpdateAsync(fetchedTeam!);

        // Assert
        var updatedTeam = await teamRepo.GetByIdAsync(teamId);
        updatedTeam.Should().NotBeNull();
        updatedTeam!.Championships.Should().Be(3);
        updatedTeam.Wins.Should().Be(10);
        updatedTeam.Losses.Should().Be(5);
        updatedTeam.Ties.Should().Be(2);
    }

    [Fact]
    public async Task UpdateTeam_WithValidFanSupportAndChemistry_UpdatesInDatabase()
    {
        // Arrange
        var leagueRepo = _fixture.ServiceProvider.GetRequiredService<ILeagueRepository>();
        var teamRepo = _fixture.ServiceProvider.GetRequiredService<ITeamRepository>();
        var teamBuilder = _fixture.ServiceProvider.GetRequiredService<ITeamBuilderService>();
        var leagueBuilder = _fixture.ServiceProvider.GetRequiredService<ILeagueBuilderService>();

        var league = leagueBuilder.CreateLeague("Test League", 1, 1, 1);
        await leagueRepo.AddAsync(league);
        var teamId = league.Conferences.First().Divisions.First().Teams.First().Id;

        // Act
        var fetchedTeam = await teamRepo.GetByIdAsync(teamId);
        teamBuilder.UpdateTeam(fetchedTeam!, null, null, null, null, null, null, null, 85, 90);
        await teamRepo.UpdateAsync(fetchedTeam!);

        // Assert
        var updatedTeam = await teamRepo.GetByIdAsync(teamId);
        updatedTeam.Should().NotBeNull();
        updatedTeam!.FanSupport.Should().Be(85);
        updatedTeam.Chemistry.Should().Be(90);
    }

    [Fact]
    public async Task UpdateTeam_WithAllParameters_UpdatesAllFieldsInDatabase()
    {
        // Arrange
        var leagueRepo = _fixture.ServiceProvider.GetRequiredService<ILeagueRepository>();
        var teamRepo = _fixture.ServiceProvider.GetRequiredService<ITeamRepository>();
        var teamBuilder = _fixture.ServiceProvider.GetRequiredService<ITeamBuilderService>();
        var leagueBuilder = _fixture.ServiceProvider.GetRequiredService<ILeagueBuilderService>();

        var league = leagueBuilder.CreateLeague("Test League", 1, 1, 1);
        await leagueRepo.AddAsync(league);
        var teamId = league.Conferences.First().Divisions.First().Teams.First().Id;

        // Act
        var fetchedTeam = await teamRepo.GetByIdAsync(teamId);
        teamBuilder.UpdateTeam(
            fetchedTeam!,
            "Completely New Name",
            "Completely New City",
            100000000,
            5, // Championships
            15, // Wins
            3, // Losses
            1, // Ties
            95, // FanSupport
            88);  // Chemistry
        await teamRepo.UpdateAsync(fetchedTeam!);

        // Assert
        var updatedTeam = await teamRepo.GetByIdAsync(teamId);
        updatedTeam.Should().NotBeNull();
        updatedTeam!.Name.Should().Be("Completely New Name");
        updatedTeam.City.Should().Be("Completely New City");
        updatedTeam.Budget.Should().Be(100000000);
        updatedTeam.Championships.Should().Be(5);
        updatedTeam.Wins.Should().Be(15);
        updatedTeam.Losses.Should().Be(3);
        updatedTeam.Ties.Should().Be(1);
        updatedTeam.FanSupport.Should().Be(95);
        updatedTeam.Chemistry.Should().Be(88);
    }

    [Fact]
    public async Task UpdateTeam_SoftDeletedEntity_ReturnsNull()
    {
        // Arrange
        var leagueRepo = _fixture.ServiceProvider.GetRequiredService<ILeagueRepository>();
        var teamRepo = _fixture.ServiceProvider.GetRequiredService<ITeamRepository>();
        var leagueBuilder = _fixture.ServiceProvider.GetRequiredService<ILeagueBuilderService>();

        var league = leagueBuilder.CreateLeague("Test League", 1, 1, 1);
        await leagueRepo.AddAsync(league);
        var teamId = league.Conferences.First().Divisions.First().Teams.First().Id;

        await teamRepo.SoftDeleteAsync(teamId, "TestUser", "Testing");

        // Act
        var fetchedTeam = await teamRepo.GetByIdAsync(teamId);

        // Assert
        fetchedTeam.Should().BeNull("because soft-deleted entities are excluded from normal queries");
    }

    // ==========================================
    // CASCADE DELETE MANAGEMENT ENDPOINT TESTS
    // ==========================================
    [Fact]
    public async Task GetDeletedLeagues_ReturnsOnlySoftDeletedLeagues()
    {
        // Arrange
        var leagueRepo = _fixture.ServiceProvider.GetRequiredService<ILeagueRepository>();

        var activeLeague = new League { Name = "Active League", Season = 2024, IsActive = true };
        var deletedLeague1 = new League { Name = "Deleted League 1", Season = 2024, IsActive = true };
        var deletedLeague2 = new League { Name = "Deleted League 2", Season = 2023, IsActive = true };

        await leagueRepo.AddAsync(activeLeague);
        await leagueRepo.AddAsync(deletedLeague1);
        await leagueRepo.AddAsync(deletedLeague2);

        await leagueRepo.SoftDeleteAsync(deletedLeague1.Id, "TestUser", "Test deletion 1");
        await leagueRepo.SoftDeleteAsync(deletedLeague2.Id, "TestUser", "Test deletion 2");

        // Act - Simulate controller GET /deleted endpoint
        var deletedLeagues = await leagueRepo.GetDeletedAsync();

        // Assert - Check that our deleted leagues are present (other tests may have created additional deleted leagues)
        deletedLeagues.Should().Contain(l => l.Name == "Deleted League 1");
        deletedLeagues.Should().Contain(l => l.Name == "Deleted League 2");
        deletedLeagues.Should().NotContain(l => l.Name == "Active League");
        deletedLeagues.Count(l => l.Name == "Deleted League 1" || l.Name == "Deleted League 2").Should().Be(2);
    }

    [Fact]
    public async Task ValidateRestore_ValidLeague_ReturnsSuccessResult()
    {
        // Arrange
        var leagueRepo = _fixture.ServiceProvider.GetRequiredService<ILeagueRepository>();
        var leagueBuilder = _fixture.ServiceProvider.GetRequiredService<ILeagueBuilderService>();

        var league = leagueBuilder.CreateLeague("Test League", 1, 1, 1);
        await leagueRepo.AddAsync(league);
        var leagueId = league.Id;

        await leagueRepo.SoftDeleteWithCascadeAsync(leagueId, "TestUser", "Testing");

        // Act - Simulate controller GET /validate-restore endpoint
        var validationResult = await leagueRepo.ValidateRestoreAsync(leagueId);

        // Assert
        validationResult.Should().NotBeNull();
        validationResult.CanRestore.Should().BeTrue();
        validationResult.ValidationErrors.Should().BeEmpty();
    }

    [Fact]
    public async Task RestoreWithCascade_SoftDeletedLeague_RestoresLeagueAndChildren()
    {
        // Arrange
        var leagueRepo = _fixture.ServiceProvider.GetRequiredService<ILeagueRepository>();
        var leagueBuilder = _fixture.ServiceProvider.GetRequiredService<ILeagueBuilderService>();

        // Create league with structure: 1 league → 1 conference → 1 division → 1 team
        var league = leagueBuilder.CreateLeague("Test League", 1, 1, 1);
        await leagueRepo.AddAsync(league);
        var leagueId = league.Id;

        // Cascade soft delete
        await leagueRepo.SoftDeleteWithCascadeAsync(leagueId, "TestUser", "Testing");

        // Verify league is deleted
        var deletedLeague = await leagueRepo.GetByIdAsync(leagueId);
        deletedLeague.Should().BeNull();

        // Act - Simulate controller POST /restore endpoint with cascade=true
        var restoreResult = await leagueRepo.RestoreWithCascadeAsync(leagueId, cascade: true);

        // Assert
        restoreResult.Should().NotBeNull();
        restoreResult.Success.Should().BeTrue();
        restoreResult.TotalEntitiesRestored.Should().BeGreaterThan(0);

        // Verify league is restored
        var restoredLeague = await leagueRepo.GetByIdAsync(leagueId);
        restoredLeague.Should().NotBeNull();
        restoredLeague!.Name.Should().Be("Test League");
    }
}

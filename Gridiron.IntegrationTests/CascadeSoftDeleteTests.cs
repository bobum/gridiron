using DataAccessLayer.Repositories;
using DomainObjects;
using FluentAssertions;
using GameManagement.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Gridiron.IntegrationTests;

/// <summary>
/// Integration tests for cascade soft delete functionality
/// Tests the complete cascade chain: League → Conference → Division → Team → Player
/// </summary>
public class CascadeSoftDeleteTests : IClassFixture<DatabaseTestFixture>
{
    private readonly DatabaseTestFixture _fixture;

    public CascadeSoftDeleteTests(DatabaseTestFixture fixture)
    {
        _fixture = fixture;
    }

    // ==========================================
    // CASCADE SOFT DELETE TESTS
    // ==========================================

    [Fact]
    public async Task CascadeDelete_League_SoftDeletesAllChildEntities()
    {
        // Arrange
        var leagueRepo = _fixture.ServiceProvider.GetRequiredService<ILeagueRepository>();
        var leagueBuilder = _fixture.ServiceProvider.GetRequiredService<ILeagueBuilderService>();

        // Create a league with structure: 1 league → 2 conferences → 2 divisions each → 2 teams each = 8 teams total
        var league = leagueBuilder.CreateLeague("Cascade Test League", 2, 2, 2);
        await leagueRepo.AddAsync(league);
        var leagueId = league.Id;

        // Count entities before cascade delete
        var initialConferences = league.Conferences.Count; // 2
        var initialDivisions = league.Conferences.SelectMany(c => c.Divisions).Count(); // 4
        var initialTeams = league.Conferences.SelectMany(c => c.Divisions).SelectMany(d => d.Teams).Count(); // 8

        initialConferences.Should().Be(2);
        initialDivisions.Should().Be(4);
        initialTeams.Should().Be(8);

        // Act - Cascade soft delete the league
        var result = await leagueRepo.SoftDeleteWithCascadeAsync(leagueId, "TestUser", "Testing cascade delete");

        // Assert - Verify cascade delete was successful
        result.Success.Should().BeTrue();
        result.TotalEntitiesDeleted.Should().Be(1 + 2 + 4 + 8); // 15 entities total (excluding players since we didn't add any)

        result.DeletedByType["Leagues"].Should().Be(1);
        result.DeletedByType["Conferences"].Should().Be(2);
        result.DeletedByType["Divisions"].Should().Be(4);
        result.DeletedByType["Teams"].Should().Be(8);
        result.DeletedByType["Players"].Should().Be(0); // No players added

        result.DeletedBy.Should().Be("TestUser");
        result.DeletionReason.Should().Be("Testing cascade delete");

        // Assert - Verify league is not findable in normal queries
        var foundLeague = await leagueRepo.GetByIdAsync(leagueId);
        foundLeague.Should().BeNull("because soft-deleted entities are excluded from queries");

        // Assert - Verify all entities are marked as deleted (using IgnoreQueryFilters)
        var deletedLeague = await _fixture.DbContext.Leagues
            .IgnoreQueryFilters()
            .Include(l => l.Conferences)
                .ThenInclude(c => c.Divisions)
                    .ThenInclude(d => d.Teams)
            .FirstOrDefaultAsync(l => l.Id == leagueId);

        deletedLeague.Should().NotBeNull();
        deletedLeague!.IsDeleted.Should().BeTrue();

        // Verify all conferences are soft-deleted
        deletedLeague.Conferences.Should().HaveCount(2);
        deletedLeague.Conferences.Should().OnlyContain(c => c.IsDeleted == true);

        // Verify all divisions are soft-deleted
        var allDivisions = deletedLeague.Conferences.SelectMany(c => c.Divisions).ToList();
        allDivisions.Should().HaveCount(4);
        allDivisions.Should().OnlyContain(d => d.IsDeleted == true);

        // Verify all teams are soft-deleted
        var allTeams = allDivisions.SelectMany(d => d.Teams).ToList();
        allTeams.Should().HaveCount(8);
        allTeams.Should().OnlyContain(t => t.IsDeleted == true);

        // Verify deletion metadata is set
        deletedLeague.DeletedBy.Should().Be("TestUser");
        deletedLeague.DeletionReason.Should().Be("Testing cascade delete");
        deletedLeague.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task CascadeDelete_League_WithPlayers_SoftDeletesPlayersAsWell()
    {
        // Arrange
        var leagueRepo = _fixture.ServiceProvider.GetRequiredService<ILeagueRepository>();
        var leagueBuilder = _fixture.ServiceProvider.GetRequiredService<ILeagueBuilderService>();
        var teamBuilder = _fixture.ServiceProvider.GetRequiredService<ITeamBuilderService>();

        // Create a league with 1 team
        var league = leagueBuilder.CreateLeague("Player Cascade Test", 1, 1, 1);

        // Add players to the team
        var team = league.Conferences[0].Divisions[0].Teams[0];
        teamBuilder.PopulateTeamRoster(team, 12345); // Adds 53 players

        await leagueRepo.AddAsync(league);
        var leagueId = league.Id;

        // Act - Cascade soft delete the league
        var result = await leagueRepo.SoftDeleteWithCascadeAsync(leagueId, "TestUser", "Testing player cascade");

        // Assert - Verify players were also deleted
        result.Success.Should().BeTrue();
        result.DeletedByType["Players"].Should().Be(53, "because we added 53 players to the team");
        result.TotalEntitiesDeleted.Should().Be(1 + 1 + 1 + 1 + 53); // League + Conference + Division + Team + Players = 57

        // Verify in database
        var deletedLeague = await _fixture.DbContext.Leagues
            .IgnoreQueryFilters()
            .Include(l => l.Conferences)
                .ThenInclude(c => c.Divisions)
                    .ThenInclude(d => d.Teams)
                        .ThenInclude(t => t.Players)
            .FirstOrDefaultAsync(l => l.Id == leagueId);

        var allPlayers = deletedLeague!.Conferences
            .SelectMany(c => c.Divisions)
            .SelectMany(d => d.Teams)
            .SelectMany(t => t.Players)
            .ToList();

        allPlayers.Should().HaveCount(53);
        allPlayers.Should().OnlyContain(p => p.IsDeleted == true, "all players should be soft-deleted");
    }

    [Fact]
    public async Task CascadeDelete_AlreadyDeletedLeague_ReturnsError()
    {
        // Arrange
        var leagueRepo = _fixture.ServiceProvider.GetRequiredService<ILeagueRepository>();
        var league = new League { Name = "Already Deleted League", Season = 2024, IsActive = true };
        await leagueRepo.AddAsync(league);
        var leagueId = league.Id;

        // First cascade delete
        await leagueRepo.SoftDeleteWithCascadeAsync(leagueId);

        // Act - Try to cascade delete again
        var result = await leagueRepo.SoftDeleteWithCascadeAsync(leagueId);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("already deleted");
    }

    [Fact]
    public async Task CascadeDelete_NonExistentLeague_ReturnsError()
    {
        // Arrange
        var leagueRepo = _fixture.ServiceProvider.GetRequiredService<ILeagueRepository>();
        var nonExistentId = 999999;

        // Act
        var result = await leagueRepo.SoftDeleteWithCascadeAsync(nonExistentId);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not found");
    }

    // ==========================================
    // CASCADE RESTORE TESTS
    // ==========================================

    [Fact]
    public async Task CascadeRestore_LeagueOnly_RestoresOnlyLeague()
    {
        // Arrange
        var leagueRepo = _fixture.ServiceProvider.GetRequiredService<ILeagueRepository>();
        var leagueBuilder = _fixture.ServiceProvider.GetRequiredService<ILeagueBuilderService>();

        var league = leagueBuilder.CreateLeague("Restore Test League", 2, 2, 2);
        await leagueRepo.AddAsync(league);
        var leagueId = league.Id;

        // Cascade delete
        await leagueRepo.SoftDeleteWithCascadeAsync(leagueId);

        // Act - Restore without cascade
        var result = await leagueRepo.RestoreWithCascadeAsync(leagueId, cascade: false);

        // Assert - Only league restored
        result.Success.Should().BeTrue();
        result.TotalEntitiesRestored.Should().Be(1);
        result.RestoredByType["Leagues"].Should().Be(1);
        result.RestoredByType.Should().NotContainKey("Conferences");

        // Should have warnings about orphaned children
        result.Warnings.Should().Contain(w => w.Contains("conferences remain soft-deleted"));
        result.Warnings.Should().Contain(w => w.Contains("divisions remain soft-deleted"));
        result.Warnings.Should().Contain(w => w.Contains("teams remain soft-deleted"));

        // Verify league is restored but children are still deleted
        var restoredLeague = await leagueRepo.GetByIdAsync(leagueId);
        restoredLeague.Should().NotBeNull();
        restoredLeague!.IsDeleted.Should().BeFalse();

        // Check children are still deleted
        var leagueWithChildren = await _fixture.DbContext.Leagues
            .IgnoreQueryFilters()
            .Include(l => l.Conferences)
            .FirstOrDefaultAsync(l => l.Id == leagueId);

        leagueWithChildren!.Conferences.Should().OnlyContain(c => c.IsDeleted == true);
    }

    [Fact]
    public async Task CascadeRestore_WithCascade_RestoresAllChildren()
    {
        // Arrange
        var leagueRepo = _fixture.ServiceProvider.GetRequiredService<ILeagueRepository>();
        var leagueBuilder = _fixture.ServiceProvider.GetRequiredService<ILeagueBuilderService>();

        var league = leagueBuilder.CreateLeague("Full Restore Test", 2, 2, 2);
        await leagueRepo.AddAsync(league);
        var leagueId = league.Id;

        // Cascade delete
        await leagueRepo.SoftDeleteWithCascadeAsync(leagueId);

        // Act - Restore with cascade
        var result = await leagueRepo.RestoreWithCascadeAsync(leagueId, cascade: true);

        // Assert - All entities restored
        result.Success.Should().BeTrue();
        result.TotalEntitiesRestored.Should().Be(15); // 1 league + 2 conferences + 4 divisions + 8 teams

        result.RestoredByType["Leagues"].Should().Be(1);
        result.RestoredByType["Conferences"].Should().Be(2);
        result.RestoredByType["Divisions"].Should().Be(4);
        result.RestoredByType["Teams"].Should().Be(8);

        // Verify all entities are restored
        var restoredLeague = await _fixture.DbContext.Leagues
            .Include(l => l.Conferences)
                .ThenInclude(c => c.Divisions)
                    .ThenInclude(d => d.Teams)
            .FirstOrDefaultAsync(l => l.Id == leagueId);

        restoredLeague.Should().NotBeNull();
        restoredLeague!.IsDeleted.Should().BeFalse();
        restoredLeague.Conferences.Should().OnlyContain(c => c.IsDeleted == false);

        var allDivisions = restoredLeague.Conferences.SelectMany(c => c.Divisions).ToList();
        allDivisions.Should().OnlyContain(d => d.IsDeleted == false);

        var allTeams = allDivisions.SelectMany(d => d.Teams).ToList();
        allTeams.Should().OnlyContain(t => t.IsDeleted == false);
    }

    [Fact]
    public async Task CascadeRestore_NonDeletedLeague_ReturnsError()
    {
        // Arrange
        var leagueRepo = _fixture.ServiceProvider.GetRequiredService<ILeagueRepository>();
        var league = new League { Name = "Active League", Season = 2024, IsActive = true };
        await leagueRepo.AddAsync(league);
        var leagueId = league.Id;

        // Act - Try to restore a non-deleted league
        var result = await leagueRepo.RestoreWithCascadeAsync(leagueId);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("is not deleted");
    }

    // ==========================================
    // RESTORE VALIDATION TESTS
    // ==========================================

    [Fact]
    public async Task ValidateRestore_DeletedLeague_WithNoOrphans_CanRestore()
    {
        // Arrange
        var leagueRepo = _fixture.ServiceProvider.GetRequiredService<ILeagueRepository>();
        var league = new League { Name = "Validation Test League", Season = 2024, IsActive = true };
        await leagueRepo.AddAsync(league);
        var leagueId = league.Id;

        await leagueRepo.SoftDeleteAsync(leagueId);

        // Act
        var result = await leagueRepo.ValidateRestoreAsync(leagueId);

        // Assert
        result.CanRestore.Should().BeTrue();
        result.ValidationErrors.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateRestore_DeletedLeague_WithOrphanedChildren_WarnsAboutOrphans()
    {
        // Arrange
        var leagueRepo = _fixture.ServiceProvider.GetRequiredService<ILeagueRepository>();
        var leagueBuilder = _fixture.ServiceProvider.GetRequiredService<ILeagueBuilderService>();

        var league = leagueBuilder.CreateLeague("Orphan Warning Test", 1, 1, 1);
        await leagueRepo.AddAsync(league);
        var leagueId = league.Id;

        // Cascade delete to create orphans
        await leagueRepo.SoftDeleteWithCascadeAsync(leagueId);

        // Act
        var result = await leagueRepo.ValidateRestoreAsync(leagueId);

        // Assert
        result.CanRestore.Should().BeTrue("league has no parents");
        result.ValidationErrors.Should().BeEmpty();

        // Should have warnings about orphaned children
        result.OrphanedChildren.Should().NotBeEmpty();
        result.OrphanedChildren["Conferences"].Should().Be(1);
        result.OrphanedChildren["Divisions"].Should().Be(1);
        result.OrphanedChildren["Teams"].Should().Be(1);

        result.Warnings.Should().Contain(w => w.Contains("child entities will remain soft-deleted"));
    }

    [Fact]
    public async Task ValidateRestore_ActiveLeague_CannotRestore()
    {
        // Arrange
        var leagueRepo = _fixture.ServiceProvider.GetRequiredService<ILeagueRepository>();
        var league = new League { Name = "Active League Validation", Season = 2024, IsActive = true };
        await leagueRepo.AddAsync(league);
        var leagueId = league.Id;

        // Act
        var result = await leagueRepo.ValidateRestoreAsync(leagueId);

        // Assert
        result.CanRestore.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Contains("is not deleted"));
    }

    [Fact]
    public async Task ValidateRestore_NonExistentLeague_CannotRestore()
    {
        // Arrange
        var leagueRepo = _fixture.ServiceProvider.GetRequiredService<ILeagueRepository>();
        var nonExistentId = 888888;

        // Act
        var result = await leagueRepo.ValidateRestoreAsync(nonExistentId);

        // Assert
        result.CanRestore.Should().BeFalse();
        result.ValidationErrors.Should().Contain(e => e.Contains("not found"));
    }
}

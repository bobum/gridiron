using DataAccessLayer.Repositories;
using DomainObjects;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Gridiron.IntegrationTests;

/// <summary>
/// Integration tests for soft delete functionality across repositories
/// Uses SQLite in-memory database to properly test EF Core query filters
/// (EF Core InMemory provider doesn't support query filters)
/// All tests verify soft-deleted entities are excluded from normal queries.
/// </summary>
public class SoftDeleteRepositoryTests : IClassFixture<DatabaseTestFixture>
{
    private readonly DatabaseTestFixture _fixture;

    public SoftDeleteRepositoryTests(DatabaseTestFixture fixture)
    {
        _fixture = fixture;
    }

    // ==========================================
    // LEAGUE REPOSITORY SOFT DELETE TESTS
    // ==========================================
    [Fact]
    public async Task LeagueRepository_SoftDelete_MarksLeagueAsDeleted()
    {
        // Arrange
        var leagueRepo = _fixture.ServiceProvider.GetRequiredService<ILeagueRepository>();
        var league = new League { Name = "Test League", Season = 2024, IsActive = true };
        await leagueRepo.AddAsync(league);
        var leagueId = league.Id;

        // Act
        await leagueRepo.SoftDeleteAsync(leagueId, "TestUser", "Test deletion");

        // Assert - Query filters should EXCLUDE soft-deleted entities (SQLite supports this!)
        var foundLeague = await leagueRepo.GetByIdAsync(leagueId);
        foundLeague.Should().BeNull("because soft-deleted entities are excluded from normal queries");

        // Assert - Using IgnoreQueryFilters should FIND it with metadata
        var deletedLeague = await _fixture.DbContext.Leagues
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(l => l.Id == leagueId);

        deletedLeague.Should().NotBeNull();
        deletedLeague!.IsDeleted.Should().BeTrue("entity should be marked as deleted");
        deletedLeague.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        deletedLeague.DeletedBy.Should().Be("TestUser");
        deletedLeague.DeletionReason.Should().Be("Test deletion");
    }

    [Fact]
    public async Task LeagueRepository_SoftDelete_ThrowsWhenAlreadyDeleted()
    {
        // Arrange
        var leagueRepo = _fixture.ServiceProvider.GetRequiredService<ILeagueRepository>();
        var league = new League { Name = "Test League 2", Season = 2024, IsActive = true };
        await leagueRepo.AddAsync(league);
        var leagueId = league.Id;
        await leagueRepo.SoftDeleteAsync(leagueId);

        // Act & Assert
        var act = async () => await leagueRepo.SoftDeleteAsync(leagueId);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"League with ID {leagueId} is already deleted");
    }

    [Fact]
    public async Task LeagueRepository_Restore_RestoresSoftDeletedLeague()
    {
        // Arrange
        var leagueRepo = _fixture.ServiceProvider.GetRequiredService<ILeagueRepository>();
        var league = new League { Name = "Test League 3", Season = 2024, IsActive = true };
        await leagueRepo.AddAsync(league);
        var leagueId = league.Id;
        await leagueRepo.SoftDeleteAsync(leagueId, "TestUser", "Test deletion");

        // Act
        await leagueRepo.RestoreAsync(leagueId);

        // Assert - Should be findable in normal queries again
        var restoredLeague = await leagueRepo.GetByIdAsync(leagueId);
        restoredLeague.Should().NotBeNull();
        restoredLeague!.IsDeleted.Should().BeFalse();
        restoredLeague.DeletedAt.Should().BeNull();
        restoredLeague.DeletedBy.Should().BeNull();
        restoredLeague.DeletionReason.Should().BeNull();
    }

    [Fact]
    public async Task LeagueRepository_Restore_ThrowsWhenNotDeleted()
    {
        // Arrange
        var leagueRepo = _fixture.ServiceProvider.GetRequiredService<ILeagueRepository>();
        var league = new League { Name = "Test League 4", Season = 2024, IsActive = true };
        await leagueRepo.AddAsync(league);
        var leagueId = league.Id;

        // Act & Assert
        var act = async () => await leagueRepo.RestoreAsync(leagueId);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"League with ID {leagueId} is not deleted");
    }

    [Fact]
    public async Task LeagueRepository_GetDeletedAsync_ReturnsOnlySoftDeletedLeagues()
    {
        // Arrange
        var leagueRepo = _fixture.ServiceProvider.GetRequiredService<ILeagueRepository>();

        var activeLeague = new League { Name = "Active League", Season = 2024, IsActive = true };
        await leagueRepo.AddAsync(activeLeague);

        var deletedLeague1 = new League { Name = "Deleted League 1", Season = 2023, IsActive = false };
        await leagueRepo.AddAsync(deletedLeague1);
        await leagueRepo.SoftDeleteAsync(deletedLeague1.Id, "User1", "Reason1");

        var deletedLeague2 = new League { Name = "Deleted League 2", Season = 2022, IsActive = false };
        await leagueRepo.AddAsync(deletedLeague2);
        await leagueRepo.SoftDeleteAsync(deletedLeague2.Id, "User2", "Reason2");

        // Act
        var deletedLeagues = await leagueRepo.GetDeletedAsync();

        // Assert
        deletedLeagues.Should().HaveCountGreaterThanOrEqualTo(2, "at least the 2 leagues we just deleted");
        deletedLeagues.Should().Contain(l => l.Id == deletedLeague1.Id);
        deletedLeagues.Should().Contain(l => l.Id == deletedLeague2.Id);
        deletedLeagues.Should().NotContain(l => l.Id == activeLeague.Id);
        deletedLeagues.Should().OnlyContain(l => l.IsDeleted == true);
    }

    // ==========================================
    // TEAM REPOSITORY SOFT DELETE TESTS
    // ==========================================
    [Fact]
    public async Task TeamRepository_SoftDelete_MarksTeamAsDeleted()
    {
        // Arrange
        var teamRepo = _fixture.ServiceProvider.GetRequiredService<ITeamRepository>();
        var team = new Team { Name = "Test Team", City = "Test City", Budget = 100000 };
        await teamRepo.AddAsync(team);
        var teamId = team.Id;

        // Act
        await teamRepo.SoftDeleteAsync(teamId, "Commissioner", "Team disbanded");

        // Assert - Query filters should exclude it
        var foundTeam = await teamRepo.GetByIdAsync(teamId);
        foundTeam.Should().BeNull("because soft-deleted teams are excluded from normal queries");

        // Assert - Verify soft delete metadata with IgnoreQueryFilters
        var deletedTeam = await _fixture.DbContext.Teams
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == teamId);

        deletedTeam.Should().NotBeNull();
        deletedTeam!.IsDeleted.Should().BeTrue("team should be marked as deleted");
        deletedTeam.DeletedBy.Should().Be("Commissioner");
        deletedTeam.DeletionReason.Should().Be("Team disbanded");
        deletedTeam.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task TeamRepository_GetDeletedAsync_ReturnsOnlySoftDeletedTeams()
    {
        // Arrange
        var teamRepo = _fixture.ServiceProvider.GetRequiredService<ITeamRepository>();

        var activeTeam = new Team { Name = "Active Team", City = "Active City", Budget = 100000 };
        await teamRepo.AddAsync(activeTeam);

        var deletedTeam = new Team { Name = "Deleted Team", City = "Deleted City", Budget = 50000 };
        await teamRepo.AddAsync(deletedTeam);
        await teamRepo.SoftDeleteAsync(deletedTeam.Id);

        // Act
        var deletedTeams = await teamRepo.GetDeletedAsync();

        // Assert
        deletedTeams.Should().Contain(t => t.Id == deletedTeam.Id);
        deletedTeams.Should().NotContain(t => t.Id == activeTeam.Id);
    }

    // ==========================================
    // GAME REPOSITORY SOFT DELETE TESTS
    // ==========================================
    [Fact]
    public async Task GameRepository_SoftDelete_MarksGameAsDeleted()
    {
        // Arrange
        var teamRepo = _fixture.ServiceProvider.GetRequiredService<ITeamRepository>();
        var gameRepo = _fixture.ServiceProvider.GetRequiredService<IGameRepository>();

        // Create two teams
        var homeTeam = new Team { Name = "Home Team", City = "Home City", Budget = 100000 };
        var awayTeam = new Team { Name = "Away Team", City = "Away City", Budget = 100000 };
        await teamRepo.AddAsync(homeTeam);
        await teamRepo.AddAsync(awayTeam);

        // Create game
        var game = new Game
        {
            HomeTeamId = homeTeam.Id,
            AwayTeamId = awayTeam.Id,
            HomeScore = 24,
            AwayScore = 17,
            RandomSeed = 12345
        };
        await gameRepo.AddAsync(game);
        var gameId = game.Id;

        // Act - Commissioner rolls back the week
        await gameRepo.SoftDeleteAsync(gameId, "Commissioner", "Week 5 rollback");

        // Assert - Query filters exclude rolled-back games
        var foundGame = await gameRepo.GetByIdAsync(gameId);
        foundGame.Should().BeNull("because soft-deleted games are excluded (rollback scenario)");

        // Assert - Verify soft delete metadata for rollback scenario
        var deletedGame = await _fixture.DbContext.Games
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(g => g.Id == gameId);

        deletedGame.Should().NotBeNull();
        deletedGame!.IsDeleted.Should().BeTrue("game should be marked as deleted for rollback");
        deletedGame.DeletedBy.Should().Be("Commissioner");
        deletedGame.DeletionReason.Should().Be("Week 5 rollback");
        deletedGame.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GameRepository_Restore_AllowsUndoingRollback()
    {
        // Arrange
        var teamRepo = _fixture.ServiceProvider.GetRequiredService<ITeamRepository>();
        var gameRepo = _fixture.ServiceProvider.GetRequiredService<IGameRepository>();

        var homeTeam = new Team { Name = "Team A", City = "City A", Budget = 100000 };
        var awayTeam = new Team { Name = "Team B", City = "City B", Budget = 100000 };
        await teamRepo.AddAsync(homeTeam);
        await teamRepo.AddAsync(awayTeam);

        var game = new Game
        {
            HomeTeamId = homeTeam.Id,
            AwayTeamId = awayTeam.Id,
            HomeScore = 21,
            AwayScore = 14
        };
        await gameRepo.AddAsync(game);
        var gameId = game.Id;

        // Soft delete (rollback)
        await gameRepo.SoftDeleteAsync(gameId, "Commissioner", "Rollback test");

        // Act - Restore the game
        await gameRepo.RestoreAsync(gameId);

        // Assert - Game should be visible again
        var restoredGame = await gameRepo.GetByIdAsync(gameId);
        restoredGame.Should().NotBeNull();
        restoredGame!.IsDeleted.Should().BeFalse();
        restoredGame.HomeScore.Should().Be(21);
        restoredGame.AwayScore.Should().Be(14);
    }

    [Fact]
    public async Task GameRepository_GetDeletedAsync_ReturnsRolledBackGames()
    {
        // Arrange
        var teamRepo = _fixture.ServiceProvider.GetRequiredService<ITeamRepository>();
        var gameRepo = _fixture.ServiceProvider.GetRequiredService<IGameRepository>();

        var team1 = new Team { Name = "Team 1", City = "City 1", Budget = 100000 };
        var team2 = new Team { Name = "Team 2", City = "City 2", Budget = 100000 };
        await teamRepo.AddAsync(team1);
        await teamRepo.AddAsync(team2);

        // Active game
        var activeGame = new Game { HomeTeamId = team1.Id, AwayTeamId = team2.Id };
        await gameRepo.AddAsync(activeGame);

        // Rolled back game
        var rolledBackGame = new Game { HomeTeamId = team1.Id, AwayTeamId = team2.Id };
        await gameRepo.AddAsync(rolledBackGame);
        await gameRepo.SoftDeleteAsync(rolledBackGame.Id, "Commissioner", "Week rollback");

        // Act
        var deletedGames = await gameRepo.GetDeletedAsync();

        // Assert
        deletedGames.Should().Contain(g => g.Id == rolledBackGame.Id);
        deletedGames.Should().NotContain(g => g.Id == activeGame.Id);
    }

    // ==========================================
    // PLAYBYPLAY REPOSITORY SOFT DELETE TESTS
    // ==========================================
    [Fact]
    public async Task PlayByPlayRepository_SoftDelete_MarksPlayByPlayAsDeleted()
    {
        // Arrange
        var teamRepo = _fixture.ServiceProvider.GetRequiredService<ITeamRepository>();
        var gameRepo = _fixture.ServiceProvider.GetRequiredService<IGameRepository>();
        var playByPlayRepo = _fixture.ServiceProvider.GetRequiredService<IPlayByPlayRepository>();

        var homeTeam = new Team { Name = "Home", City = "City", Budget = 100000 };
        var awayTeam = new Team { Name = "Away", City = "City", Budget = 100000 };
        await teamRepo.AddAsync(homeTeam);
        await teamRepo.AddAsync(awayTeam);

        var game = new Game { HomeTeamId = homeTeam.Id, AwayTeamId = awayTeam.Id };
        await gameRepo.AddAsync(game);

        var playByPlay = new PlayByPlay
        {
            Game = game,
            GameId = game.Id,
            PlaysJson = "[{\"play\": \"data\"}]",
            PlayByPlayLog = "Test log"
        };
        await playByPlayRepo.AddAsync(playByPlay);
        var playByPlayId = playByPlay.Id;

        // Act
        await playByPlayRepo.SoftDeleteAsync(playByPlayId, "System", "Game rollback");

        // Assert - Verify soft delete metadata
        var deleted = await _fixture.DbContext.PlayByPlays
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == playByPlayId);

        deleted.Should().NotBeNull();
        deleted!.IsDeleted.Should().BeTrue("playbyplay should be marked as deleted");
        deleted.DeletionReason.Should().Be("Game rollback");
        deleted.DeletedBy.Should().Be("System");
        deleted.DeletedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    // ==========================================
    // QUERY FILTER TESTS
    // ==========================================
    // SQLite in-memory supports query filters (unlike EF Core InMemory provider)
    // These tests verify soft-deleted entities are properly excluded from normal queries
    // Note: Repository GetByIdAsync methods use FirstOrDefaultAsync (not FindAsync)
    // because FindAsync bypasses EF Core query filters
    [Fact]
    public async Task QueryFilters_ExcludeSoftDeletedEntitiesFromNormalQueries()
    {
        // Arrange
        var leagueRepo = _fixture.ServiceProvider.GetRequiredService<ILeagueRepository>();

        var league1 = new League { Name = "Active League 1", Season = 2024 };
        var league2 = new League { Name = "Deleted League", Season = 2023 };
        var league3 = new League { Name = "Active League 2", Season = 2024 };

        await leagueRepo.AddAsync(league1);
        await leagueRepo.AddAsync(league2);
        await leagueRepo.AddAsync(league3);

        await leagueRepo.SoftDeleteAsync(league2.Id);

        // Act - Normal query should exclude soft-deleted entities
        var activeLeagues = await leagueRepo.GetAllAsync();

        // Assert - Query filters WORK with SQLite!
        activeLeagues.Should().Contain(l => l.Id == league1.Id);
        activeLeagues.Should().Contain(l => l.Id == league3.Id);
        activeLeagues.Should().NotContain(l => l.Id == league2.Id, "soft-deleted league should be excluded by query filter");
    }

    [Fact]
    public async Task QueryFilters_IgnoreQueryFiltersCanAccessSoftDeletedEntities()
    {
        // Arrange
        var teamRepo = _fixture.ServiceProvider.GetRequiredService<ITeamRepository>();

        var team = new Team { Name = "Soft Deleted Team", City = "City", Budget = 100000 };
        await teamRepo.AddAsync(team);
        await teamRepo.SoftDeleteAsync(team.Id);

        // Act - Use IgnoreQueryFilters to access soft-deleted entity
        var deletedTeam = await _fixture.DbContext.Teams
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == team.Id);

        // Assert
        deletedTeam.Should().NotBeNull();
        deletedTeam!.IsDeleted.Should().BeTrue("team should be marked as deleted");
    }

    // ==========================================
    // SOFT DELETE WITHOUT METADATA TESTS
    // ==========================================
    [Fact]
    public async Task SoftDelete_WorksWithoutOptionalMetadata()
    {
        // Arrange
        var leagueRepo = _fixture.ServiceProvider.GetRequiredService<ILeagueRepository>();
        var league = new League { Name = "League No Metadata", Season = 2024 };
        await leagueRepo.AddAsync(league);

        // Act - Call without deletedBy or reason
        await leagueRepo.SoftDeleteAsync(league.Id);

        // Assert
        var deleted = await _fixture.DbContext.Leagues
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(l => l.Id == league.Id);

        deleted.Should().NotBeNull();
        deleted!.IsDeleted.Should().BeTrue();
        deleted.DeletedAt.Should().NotBeNull();
        deleted.DeletedBy.Should().BeNull();
        deleted.DeletionReason.Should().BeNull();
    }
}

using DataAccessLayer.Repositories;
using DomainObjects;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Gridiron.IntegrationTests;

[Collection("Database Collection")]
public class PlayerGameStatRepositoryTests : IClassFixture<DatabaseTestFixture>
{
    private readonly DatabaseTestFixture _fixture;

    public PlayerGameStatRepositoryTests(DatabaseTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task AddRangeAsync_ShouldAddStats()
    {
        using var scope = _fixture.ServiceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IPlayerGameStatRepository>();
        var gameRepo = scope.ServiceProvider.GetRequiredService<IGameRepository>();
        var playerRepo = scope.ServiceProvider.GetRequiredService<IPlayerRepository>();
        var teamRepo = scope.ServiceProvider.GetRequiredService<ITeamRepository>();

        // Setup dependencies
        var team = new Team { Name = "Stat Team" };
        await teamRepo.AddAsync(team);

        var player = new Player { FirstName = "Stat", LastName = "Player", TeamId = team.Id };
        await playerRepo.AddAsync(player);

        var game = new Game { HomeTeamId = team.Id, AwayTeamId = team.Id }; // Self game for simplicity
        await gameRepo.AddAsync(game);

        var stats = new List<PlayerGameStat>
        {
            new PlayerGameStat
            {
                PlayerId = player.Id,
                GameId = game.Id,
                Stats = new Dictionary<DomainObjects.StatTypes.PlayerStatType, int>
                {
                    { DomainObjects.StatTypes.PlayerStatType.PassingYards, 100 },
                    { DomainObjects.StatTypes.PlayerStatType.PassingTouchdowns, 1 }
                }
            }
        };

        // Act
        await repo.AddRangeAsync(stats);

        // Assert
        var retrieved = await repo.GetByGameIdAsync(game.Id);
        retrieved.Should().HaveCount(1);
        retrieved[0].Stats[DomainObjects.StatTypes.PlayerStatType.PassingYards].Should().Be(100);
    }

    [Fact]
    public async Task DeleteByGameIdAsync_ShouldSoftDeleteStats()
    {
        using var scope = _fixture.ServiceProvider.CreateScope();
        var repo = scope.ServiceProvider.GetRequiredService<IPlayerGameStatRepository>();
        var gameRepo = scope.ServiceProvider.GetRequiredService<IGameRepository>();
        var playerRepo = scope.ServiceProvider.GetRequiredService<IPlayerRepository>();
        var teamRepo = scope.ServiceProvider.GetRequiredService<ITeamRepository>();

        // Setup dependencies
        var team = new Team { Name = "Delete Team" };
        await teamRepo.AddAsync(team);

        var player = new Player { FirstName = "Delete", LastName = "Player", TeamId = team.Id };
        await playerRepo.AddAsync(player);

        var game = new Game { HomeTeamId = team.Id, AwayTeamId = team.Id };
        await gameRepo.AddAsync(game);

        var stat = new PlayerGameStat
        {
            PlayerId = player.Id,
            GameId = game.Id,
            Stats = new Dictionary<DomainObjects.StatTypes.PlayerStatType, int> { { DomainObjects.StatTypes.PlayerStatType.RushingYards, 50 } }
        };
        await repo.AddAsync(stat);

        // Act
        await repo.DeleteByGameIdAsync(game.Id);

        // Assert
        var retrieved = await repo.GetByGameIdAsync(game.Id);
        retrieved.Should().BeEmpty(); // Should be filtered out by query filter or repository logic

        // Verify soft delete in DB context directly
        var context = scope.ServiceProvider.GetRequiredService<DataAccessLayer.GridironDbContext>();
        var deletedStat = await context.PlayerGameStats.AsQueryable().IgnoreQueryFilters().FirstOrDefaultAsync(s => s.Id == stat.Id);
        deletedStat.Should().NotBeNull();
        deletedStat!.IsDeleted.Should().BeTrue();
        deletedStat.DeletedBy.Should().Be("system");
    }
}

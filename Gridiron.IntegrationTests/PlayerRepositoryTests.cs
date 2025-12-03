using DataAccessLayer.Repositories;
using DomainObjects;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Gridiron.IntegrationTests;

/// <summary>
/// Integration tests for PlayerRepository
/// Tests the full stack: Repository → Database
/// Uses SQLite in-memory database to properly test EF Core behavior.
/// </summary>
[Trait("Category", "Integration")]
public class PlayerRepositoryTests : IClassFixture<DatabaseTestFixture>
{
    private readonly DatabaseTestFixture _fixture;

    public PlayerRepositoryTests(DatabaseTestFixture fixture)
    {
        _fixture = fixture;
    }

    private static Player CreateTestPlayer(string firstName, string lastName, int? teamId = null)
    {
        return new Player
        {
            FirstName = firstName,
            LastName = lastName,
            Position = Positions.QB,
            Number = 12,
            Height = "6-2",
            Weight = 220,
            Age = 25,
            Exp = 3,
            College = "Test University",
            Speed = 85,
            Strength = 80,
            Agility = 82,
            Awareness = 88,
            Morale = 90,
            Passing = 92,
            Catching = 50,
            Rushing = 65,
            Blocking = 30,
            Tackling = 20,
            Coverage = 25,
            Kicking = 15,
            Potential = 95,
            Progression = 80,
            Health = 100,
            Discipline = 85,
            TeamId = teamId
        };
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => new PlayerRepository(null!);
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("context");
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WhenPlayersExist_ReturnsAllPlayers()
    {
        // Arrange
        var playerRepo = _fixture.ServiceProvider.GetRequiredService<IPlayerRepository>();
        var player1 = CreateTestPlayer("John", "Doe");
        var player2 = CreateTestPlayer("Jane", "Smith");

        await playerRepo.AddAsync(player1);
        await playerRepo.AddAsync(player2);

        // Act
        var result = await playerRepo.GetAllAsync();

        // Assert
        result.Should().Contain(p => p.FirstName == "John" && p.LastName == "Doe");
        result.Should().Contain(p => p.FirstName == "Jane" && p.LastName == "Smith");
    }

    [Fact]
    public async Task GetAllAsync_ReturnsListType()
    {
        // Arrange
        var playerRepo = _fixture.ServiceProvider.GetRequiredService<IPlayerRepository>();

        // Act
        var result = await playerRepo.GetAllAsync();

        // Assert
        result.Should().BeOfType<List<Player>>();
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithExistingPlayer_ReturnsPlayer()
    {
        // Arrange
        var playerRepo = _fixture.ServiceProvider.GetRequiredService<IPlayerRepository>();
        var player = CreateTestPlayer("Mike", "Johnson");
        await playerRepo.AddAsync(player);

        // Act
        var result = await playerRepo.GetByIdAsync(player.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(player.Id);
        result.FirstName.Should().Be("Mike");
        result.LastName.Should().Be("Johnson");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        var playerRepo = _fixture.ServiceProvider.GetRequiredService<IPlayerRepository>();

        // Act
        var result = await playerRepo.GetByIdAsync(99999);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetByTeamIdAsync Tests

    [Fact]
    public async Task GetByTeamIdAsync_WithPlayersOnTeam_ReturnsTeamPlayers()
    {
        // Arrange
        var playerRepo = _fixture.ServiceProvider.GetRequiredService<IPlayerRepository>();
        var teamRepo = _fixture.ServiceProvider.GetRequiredService<ITeamRepository>();

        // Create a team first
        var team = new Team { Name = "TestTeam", City = "TestCity" };
        await teamRepo.AddAsync(team);

        // Create players for this team
        var player1 = CreateTestPlayer("Tom", "Brady", team.Id);
        var player2 = CreateTestPlayer("Rob", "Gronkowski", team.Id);
        var player3 = CreateTestPlayer("Other", "Player", null); // Different team

        await playerRepo.AddAsync(player1);
        await playerRepo.AddAsync(player2);
        await playerRepo.AddAsync(player3);

        // Act
        var result = await playerRepo.GetByTeamIdAsync(team.Id);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.FirstName == "Tom" && p.LastName == "Brady");
        result.Should().Contain(p => p.FirstName == "Rob" && p.LastName == "Gronkowski");
        result.Should().NotContain(p => p.FirstName == "Other");
    }

    [Fact]
    public async Task GetByTeamIdAsync_WithNoPlayersOnTeam_ReturnsEmptyList()
    {
        // Arrange
        var playerRepo = _fixture.ServiceProvider.GetRequiredService<IPlayerRepository>();

        // Act
        var result = await playerRepo.GetByTeamIdAsync(88888);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_WithValidPlayer_AddsPlayerToDatabase()
    {
        // Arrange
        var playerRepo = _fixture.ServiceProvider.GetRequiredService<IPlayerRepository>();
        var player = CreateTestPlayer("New", "Player");

        // Act
        var result = await playerRepo.AddAsync(player);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);

        // Verify it was persisted
        var retrieved = await playerRepo.GetByIdAsync(result.Id);
        retrieved.Should().NotBeNull();
        retrieved!.FirstName.Should().Be("New");
    }

    [Fact]
    public async Task AddAsync_SetsPlayerSkillsCorrectly()
    {
        // Arrange
        var playerRepo = _fixture.ServiceProvider.GetRequiredService<IPlayerRepository>();
        var player = CreateTestPlayer("Skill", "Test");
        player.Passing = 95;
        player.Rushing = 88;

        // Act
        var result = await playerRepo.AddAsync(player);

        // Assert
        var retrieved = await playerRepo.GetByIdAsync(result.Id);
        retrieved!.Passing.Should().Be(95);
        retrieved.Rushing.Should().Be(88);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithExistingPlayer_UpdatesPlayerInDatabase()
    {
        // Arrange
        var playerRepo = _fixture.ServiceProvider.GetRequiredService<IPlayerRepository>();
        var player = CreateTestPlayer("Update", "Test");
        await playerRepo.AddAsync(player);

        // Act
        player.FirstName = "Updated";
        player.Passing = 99;
        await playerRepo.UpdateAsync(player);

        // Assert
        var retrieved = await playerRepo.GetByIdAsync(player.Id);
        retrieved.Should().NotBeNull();
        retrieved!.FirstName.Should().Be("Updated");
        retrieved.Passing.Should().Be(99);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesMultipleFields()
    {
        // Arrange
        var playerRepo = _fixture.ServiceProvider.GetRequiredService<IPlayerRepository>();
        var player = CreateTestPlayer("Multi", "Update");
        await playerRepo.AddAsync(player);

        // Act
        player.Age = 30;
        player.Exp = 8;
        player.Speed = 75;
        player.Strength = 90;
        await playerRepo.UpdateAsync(player);

        // Assert
        var retrieved = await playerRepo.GetByIdAsync(player.Id);
        retrieved!.Age.Should().Be(30);
        retrieved.Exp.Should().Be(8);
        retrieved.Speed.Should().Be(75);
        retrieved.Strength.Should().Be(90);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WithExistingPlayer_RemovesPlayerFromDatabase()
    {
        // Arrange
        var playerRepo = _fixture.ServiceProvider.GetRequiredService<IPlayerRepository>();
        var player = CreateTestPlayer("Delete", "Me");
        await playerRepo.AddAsync(player);
        var playerId = player.Id;

        // Act
        await playerRepo.DeleteAsync(playerId);

        // Assert
        var retrieved = await playerRepo.GetByIdAsync(playerId);
        retrieved.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentPlayer_DoesNotThrow()
    {
        // Arrange
        var playerRepo = _fixture.ServiceProvider.GetRequiredService<IPlayerRepository>();

        // Act & Assert - should not throw
        var act = async () => await playerRepo.DeleteAsync(77777);
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region SaveChangesAsync Tests

    [Fact]
    public async Task SaveChangesAsync_ReturnsNumberOfChanges()
    {
        // Arrange
        var playerRepo = _fixture.ServiceProvider.GetRequiredService<IPlayerRepository>();

        // Act - SaveChangesAsync is called internally by AddAsync, but we test direct call
        var result = await playerRepo.SaveChangesAsync();

        // Assert
        result.Should().BeGreaterThanOrEqualTo(0);
    }

    #endregion

    #region Player Properties Tests

    [Fact]
    public async Task AddAsync_PreservesAllPlayerProperties()
    {
        // Arrange
        var playerRepo = _fixture.ServiceProvider.GetRequiredService<IPlayerRepository>();
        var player = new Player
        {
            FirstName = "Complete",
            LastName = "Player",
            Position = Positions.WR,
            Number = 84,
            Height = "6-4",
            Weight = 215,
            Age = 28,
            Exp = 6,
            College = "Stanford",
            Speed = 95,
            Strength = 75,
            Agility = 92,
            Awareness = 85,
            Morale = 88,
            Passing = 20,
            Catching = 96,
            Rushing = 60,
            Blocking = 45,
            Tackling = 30,
            Coverage = 35,
            Kicking = 10,
            Potential = 90,
            Progression = 75,
            Health = 95,
            Discipline = 80
        };

        // Act
        await playerRepo.AddAsync(player);
        var retrieved = await playerRepo.GetByIdAsync(player.Id);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.FirstName.Should().Be("Complete");
        retrieved.LastName.Should().Be("Player");
        retrieved.Position.Should().Be(Positions.WR);
        retrieved.Number.Should().Be(84);
        retrieved.Height.Should().Be("6-4");
        retrieved.Weight.Should().Be(215);
        retrieved.Age.Should().Be(28);
        retrieved.Exp.Should().Be(6);
        retrieved.College.Should().Be("Stanford");
        retrieved.Speed.Should().Be(95);
        retrieved.Strength.Should().Be(75);
        retrieved.Agility.Should().Be(92);
        retrieved.Awareness.Should().Be(85);
        retrieved.Morale.Should().Be(88);
        retrieved.Passing.Should().Be(20);
        retrieved.Catching.Should().Be(96);
        retrieved.Rushing.Should().Be(60);
        retrieved.Blocking.Should().Be(45);
        retrieved.Tackling.Should().Be(30);
        retrieved.Coverage.Should().Be(35);
        retrieved.Kicking.Should().Be(10);
        retrieved.Potential.Should().Be(90);
        retrieved.Progression.Should().Be(75);
        retrieved.Health.Should().Be(95);
        retrieved.Discipline.Should().Be(80);
    }

    #endregion
}

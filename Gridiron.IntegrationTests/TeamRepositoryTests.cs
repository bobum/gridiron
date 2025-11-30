using DataAccessLayer.Repositories;
using DomainObjects;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Gridiron.IntegrationTests;

/// <summary>
/// Integration tests for TeamRepository
/// Tests the full stack: Repository → Database
/// Uses SQLite in-memory database to properly test EF Core behavior
/// </summary>
[Trait("Category", "Integration")]
public class TeamRepositoryTests : IClassFixture<DatabaseTestFixture>
{
    private readonly DatabaseTestFixture _fixture;

    public TeamRepositoryTests(DatabaseTestFixture fixture)
    {
        _fixture = fixture;
    }

    private static Team CreateTestTeam(string city, string name, int? divisionId = null)
    {
        return new Team
        {
            City = city,
            Name = name,
            Budget = 100000000,
            Championships = 3,
            Wins = 10,
            Losses = 6,
            Ties = 0,
            FanSupport = 85,
            Chemistry = 75,
            DivisionId = divisionId
        };
    }

    private static Player CreateTestPlayer(string firstName, string lastName)
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
            Discipline = 85
        };
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        var act = () => new TeamRepository(null!);
        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("context");
    }

    #endregion

    #region GetAllAsync Tests

    [Fact]
    public async Task GetAllAsync_WhenTeamsExist_ReturnsAllTeams()
    {
        // Arrange
        var teamRepo = _fixture.ServiceProvider.GetRequiredService<ITeamRepository>();
        var team1 = CreateTestTeam("TestCity1", "Wolves");
        var team2 = CreateTestTeam("TestCity2", "Bears");

        await teamRepo.AddAsync(team1);
        await teamRepo.AddAsync(team2);

        // Act
        var result = await teamRepo.GetAllAsync();

        // Assert
        result.Should().Contain(t => t.City == "TestCity1" && t.Name == "Wolves");
        result.Should().Contain(t => t.City == "TestCity2" && t.Name == "Bears");
    }

    [Fact]
    public async Task GetAllAsync_ReturnsListType()
    {
        // Arrange
        var teamRepo = _fixture.ServiceProvider.GetRequiredService<ITeamRepository>();

        // Act
        var result = await teamRepo.GetAllAsync();

        // Assert
        result.Should().BeOfType<List<Team>>();
    }

    #endregion

    #region GetByIdAsync Tests

    [Fact]
    public async Task GetByIdAsync_WithExistingTeam_ReturnsTeam()
    {
        // Arrange
        var teamRepo = _fixture.ServiceProvider.GetRequiredService<ITeamRepository>();
        var team = CreateTestTeam("GetById", "Tigers");
        await teamRepo.AddAsync(team);

        // Act
        var result = await teamRepo.GetByIdAsync(team.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(team.Id);
        result.City.Should().Be("GetById");
        result.Name.Should().Be("Tigers");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        var teamRepo = _fixture.ServiceProvider.GetRequiredService<ITeamRepository>();

        // Act
        var result = await teamRepo.GetByIdAsync(99999);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetByIdWithPlayersAsync Tests

    [Fact]
    public async Task GetByIdWithPlayersAsync_WithTeamWithPlayers_ReturnsTeamWithPlayers()
    {
        // Arrange
        var teamRepo = _fixture.ServiceProvider.GetRequiredService<ITeamRepository>();
        var playerRepo = _fixture.ServiceProvider.GetRequiredService<IPlayerRepository>();

        var team = CreateTestTeam("WithPlayers", "Lions");
        await teamRepo.AddAsync(team);

        var player1 = CreateTestPlayer("Player", "One");
        player1.TeamId = team.Id;
        var player2 = CreateTestPlayer("Player", "Two");
        player2.TeamId = team.Id;

        await playerRepo.AddAsync(player1);
        await playerRepo.AddAsync(player2);

        // Act
        var result = await teamRepo.GetByIdWithPlayersAsync(team.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Players.Should().HaveCount(2);
        result.Players.Should().Contain(p => p.FirstName == "Player" && p.LastName == "One");
        result.Players.Should().Contain(p => p.FirstName == "Player" && p.LastName == "Two");
    }

    [Fact]
    public async Task GetByIdWithPlayersAsync_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        var teamRepo = _fixture.ServiceProvider.GetRequiredService<ITeamRepository>();

        // Act
        var result = await teamRepo.GetByIdWithPlayersAsync(88888);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetByCityAndNameAsync Tests

    [Fact]
    public async Task GetByCityAndNameAsync_WithExistingTeam_ReturnsTeam()
    {
        // Arrange
        var teamRepo = _fixture.ServiceProvider.GetRequiredService<ITeamRepository>();
        var team = CreateTestTeam("Dallas", "Cowboys");
        await teamRepo.AddAsync(team);

        // Act
        var result = await teamRepo.GetByCityAndNameAsync("Dallas", "Cowboys");

        // Assert
        result.Should().NotBeNull();
        result!.City.Should().Be("Dallas");
        result.Name.Should().Be("Cowboys");
    }

    [Fact]
    public async Task GetByCityAndNameAsync_WithNonExistentTeam_ReturnsNull()
    {
        // Arrange
        var teamRepo = _fixture.ServiceProvider.GetRequiredService<ITeamRepository>();

        // Act
        var result = await teamRepo.GetByCityAndNameAsync("NonExistent", "Team");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByCityAndNameAsync_IncludesPlayers()
    {
        // Arrange
        var teamRepo = _fixture.ServiceProvider.GetRequiredService<ITeamRepository>();
        var playerRepo = _fixture.ServiceProvider.GetRequiredService<IPlayerRepository>();

        var team = CreateTestTeam("Miami", "Dolphins");
        await teamRepo.AddAsync(team);

        var player = CreateTestPlayer("Dan", "Marino");
        player.TeamId = team.Id;
        await playerRepo.AddAsync(player);

        // Act
        var result = await teamRepo.GetByCityAndNameAsync("Miami", "Dolphins");

        // Assert
        result.Should().NotBeNull();
        result!.Players.Should().Contain(p => p.FirstName == "Dan" && p.LastName == "Marino");
    }

    #endregion

    #region GetTeamsByLeagueIdAsync Tests

    [Fact]
    public async Task GetTeamsByLeagueIdAsync_WithTeamsInLeague_ReturnsTeams()
    {
        // Arrange
        var leagueRepo = _fixture.ServiceProvider.GetRequiredService<ILeagueRepository>();
        var teamRepo = _fixture.ServiceProvider.GetRequiredService<ITeamRepository>();

        // Create full hierarchy: League -> Conference -> Division -> Teams
        var league = new League { Name = "TestLeagueForTeams" };
        await leagueRepo.AddAsync(league);
        await leagueRepo.SaveChangesAsync();

        var conference = new Conference { Name = "TestConf", LeagueId = league.Id };
        league.Conferences.Add(conference);
        await leagueRepo.UpdateAsync(league);
        await leagueRepo.SaveChangesAsync();

        var division = new Division { Name = "TestDiv", ConferenceId = conference.Id };
        conference.Divisions.Add(division);
        await leagueRepo.UpdateAsync(league);
        await leagueRepo.SaveChangesAsync();

        var team1 = CreateTestTeam("LeagueTest1", "Hawks");
        team1.DivisionId = division.Id;
        var team2 = CreateTestTeam("LeagueTest2", "Eagles");
        team2.DivisionId = division.Id;

        await teamRepo.AddAsync(team1);
        await teamRepo.AddAsync(team2);

        // Act
        var result = await teamRepo.GetTeamsByLeagueIdAsync(league.Id);

        // Assert
        result.Should().Contain(t => t.City == "LeagueTest1" && t.Name == "Hawks");
        result.Should().Contain(t => t.City == "LeagueTest2" && t.Name == "Eagles");
    }

    [Fact]
    public async Task GetTeamsByLeagueIdAsync_WithNoTeamsInLeague_ReturnsEmptyList()
    {
        // Arrange
        var teamRepo = _fixture.ServiceProvider.GetRequiredService<ITeamRepository>();

        // Act
        var result = await teamRepo.GetTeamsByLeagueIdAsync(77777);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region AddAsync Tests

    [Fact]
    public async Task AddAsync_WithValidTeam_AddsTeamToDatabase()
    {
        // Arrange
        var teamRepo = _fixture.ServiceProvider.GetRequiredService<ITeamRepository>();
        var team = CreateTestTeam("NewCity", "NewTeam");

        // Act
        var result = await teamRepo.AddAsync(team);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);

        var retrieved = await teamRepo.GetByIdAsync(result.Id);
        retrieved.Should().NotBeNull();
        retrieved!.City.Should().Be("NewCity");
    }

    [Fact]
    public async Task AddAsync_PreservesAllTeamProperties()
    {
        // Arrange
        var teamRepo = _fixture.ServiceProvider.GetRequiredService<ITeamRepository>();
        var team = new Team
        {
            City = "Complete",
            Name = "Team",
            Budget = 150000000,
            Championships = 5,
            Wins = 12,
            Losses = 4,
            Ties = 1,
            FanSupport = 95,
            Chemistry = 88
        };

        // Act
        await teamRepo.AddAsync(team);
        var retrieved = await teamRepo.GetByIdAsync(team.Id);

        // Assert
        retrieved!.City.Should().Be("Complete");
        retrieved.Name.Should().Be("Team");
        retrieved.Budget.Should().Be(150000000);
        retrieved.Championships.Should().Be(5);
        retrieved.Wins.Should().Be(12);
        retrieved.Losses.Should().Be(4);
        retrieved.Ties.Should().Be(1);
        retrieved.FanSupport.Should().Be(95);
        retrieved.Chemistry.Should().Be(88);
    }

    #endregion

    #region UpdateAsync Tests

    [Fact]
    public async Task UpdateAsync_WithExistingTeam_UpdatesTeamInDatabase()
    {
        // Arrange
        var teamRepo = _fixture.ServiceProvider.GetRequiredService<ITeamRepository>();
        var team = CreateTestTeam("UpdateCity", "UpdateTeam");
        await teamRepo.AddAsync(team);

        // Act
        team.City = "UpdatedCity";
        team.Wins = 15;
        await teamRepo.UpdateAsync(team);

        // Assert
        var retrieved = await teamRepo.GetByIdAsync(team.Id);
        retrieved.Should().NotBeNull();
        retrieved!.City.Should().Be("UpdatedCity");
        retrieved.Wins.Should().Be(15);
    }

    #endregion

    #region DeleteAsync Tests

    [Fact]
    public async Task DeleteAsync_WithExistingTeam_RemovesTeamFromDatabase()
    {
        // Arrange
        var teamRepo = _fixture.ServiceProvider.GetRequiredService<ITeamRepository>();
        var team = CreateTestTeam("DeleteCity", "DeleteTeam");
        await teamRepo.AddAsync(team);
        var teamId = team.Id;

        // Act
        await teamRepo.DeleteAsync(teamId);

        // Assert
        var retrieved = await teamRepo.GetByIdAsync(teamId);
        retrieved.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistentTeam_DoesNotThrow()
    {
        // Arrange
        var teamRepo = _fixture.ServiceProvider.GetRequiredService<ITeamRepository>();

        // Act & Assert
        var act = async () => await teamRepo.DeleteAsync(66666);
        await act.Should().NotThrowAsync();
    }

    #endregion

    #region SoftDeleteAsync Tests

    [Fact]
    public async Task SoftDeleteAsync_WithExistingTeam_MarksTeamAsDeleted()
    {
        // Arrange
        var teamRepo = _fixture.ServiceProvider.GetRequiredService<ITeamRepository>();
        var team = CreateTestTeam("SoftDelete", "Team");
        await teamRepo.AddAsync(team);

        // Act
        await teamRepo.SoftDeleteAsync(team.Id, "TestUser", "Testing soft delete");

        // Assert - Team should not appear in regular queries
        var regularQuery = await teamRepo.GetByIdAsync(team.Id);
        regularQuery.Should().BeNull();

        // But should appear in deleted list
        var deletedList = await teamRepo.GetDeletedAsync();
        deletedList.Should().Contain(t => t.Id == team.Id);
    }

    [Fact]
    public async Task SoftDeleteAsync_WithNonExistentTeam_ThrowsInvalidOperationException()
    {
        // Arrange
        var teamRepo = _fixture.ServiceProvider.GetRequiredService<ITeamRepository>();

        // Act & Assert
        var act = async () => await teamRepo.SoftDeleteAsync(55555);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task SoftDeleteAsync_WithAlreadyDeletedTeam_ThrowsInvalidOperationException()
    {
        // Arrange
        var teamRepo = _fixture.ServiceProvider.GetRequiredService<ITeamRepository>();
        var team = CreateTestTeam("AlreadyDeleted", "Team");
        await teamRepo.AddAsync(team);
        await teamRepo.SoftDeleteAsync(team.Id);

        // Act & Assert
        var act = async () => await teamRepo.SoftDeleteAsync(team.Id);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already deleted*");
    }

    #endregion

    #region RestoreAsync Tests

    [Fact]
    public async Task RestoreAsync_WithSoftDeletedTeam_RestoresTeam()
    {
        // Arrange
        var teamRepo = _fixture.ServiceProvider.GetRequiredService<ITeamRepository>();
        var team = CreateTestTeam("Restore", "Team");
        await teamRepo.AddAsync(team);
        await teamRepo.SoftDeleteAsync(team.Id);

        // Act
        await teamRepo.RestoreAsync(team.Id);

        // Assert - Team should appear in regular queries again
        var regularQuery = await teamRepo.GetByIdAsync(team.Id);
        regularQuery.Should().NotBeNull();
        regularQuery!.City.Should().Be("Restore");
    }

    [Fact]
    public async Task RestoreAsync_WithNonExistentTeam_ThrowsInvalidOperationException()
    {
        // Arrange
        var teamRepo = _fixture.ServiceProvider.GetRequiredService<ITeamRepository>();

        // Act & Assert
        var act = async () => await teamRepo.RestoreAsync(44444);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task RestoreAsync_WithNotDeletedTeam_ThrowsInvalidOperationException()
    {
        // Arrange
        var teamRepo = _fixture.ServiceProvider.GetRequiredService<ITeamRepository>();
        var team = CreateTestTeam("NotDeleted", "Team");
        await teamRepo.AddAsync(team);

        // Act & Assert
        var act = async () => await teamRepo.RestoreAsync(team.Id);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not deleted*");
    }

    #endregion

    #region GetDeletedAsync Tests

    [Fact]
    public async Task GetDeletedAsync_ReturnsOnlyDeletedTeams()
    {
        // Arrange
        var teamRepo = _fixture.ServiceProvider.GetRequiredService<ITeamRepository>();
        var activeTeam = CreateTestTeam("Active", "Team");
        var deletedTeam = CreateTestTeam("Deleted", "Team");

        await teamRepo.AddAsync(activeTeam);
        await teamRepo.AddAsync(deletedTeam);
        await teamRepo.SoftDeleteAsync(deletedTeam.Id);

        // Act
        var result = await teamRepo.GetDeletedAsync();

        // Assert
        result.Should().Contain(t => t.Id == deletedTeam.Id);
        result.Should().NotContain(t => t.Id == activeTeam.Id);
    }

    #endregion

    #region SaveChangesAsync Tests

    [Fact]
    public async Task SaveChangesAsync_ReturnsNumberOfChanges()
    {
        // Arrange
        var teamRepo = _fixture.ServiceProvider.GetRequiredService<ITeamRepository>();

        // Act
        var result = await teamRepo.SaveChangesAsync();

        // Assert
        result.Should().BeGreaterThanOrEqualTo(0);
    }

    #endregion
}

using DomainObjects;
using FluentAssertions;
using GameManagement.Mapping;
using Xunit;

namespace GameManagement.Tests.Mapping;

public class GridironMapperTests
{
    private readonly GridironMapper _mapper = new ();

    [Fact]
    public void ToEngineTeam_MapsBasicProperties()
    {
        // Arrange
        var team = new Team
        {
            Id = 1,
            Name = "Test Hawks",
            City = "Test City",
            DivisionId = 5,
            Wins = 10,
            Losses = 5,
            Ties = 1,
            Budget = 1000000,
            Championships = 2,
            FanSupport = 85,
            Chemistry = 90
        };

        // Act
        var engineTeam = _mapper.ToEngineTeam(team);

        // Assert
        engineTeam.Name.Should().Be("Test Hawks");
        engineTeam.City.Should().Be("Test City");
        engineTeam.Wins.Should().Be(10);
        engineTeam.Losses.Should().Be(5);
        engineTeam.Ties.Should().Be(1);
        engineTeam.Budget.Should().Be(1000000);
        engineTeam.Championships.Should().Be(2);
        engineTeam.FanSupport.Should().Be(85);
        engineTeam.Chemistry.Should().Be(90);
    }

    [Fact]
    public void ToEngineTeam_MapsPlayers()
    {
        // Arrange
        var team = new Team { Name = "Test Team" };
        team.Players.Add(new Player
        {
            Id = 1,
            TeamId = 1,
            FirstName = "John",
            LastName = "Quarterback",
            Position = Positions.QB,
            Number = 12,
            Speed = 75,
            Strength = 70,
            Agility = 80,
            Awareness = 85,
            Passing = 90,
            Health = 100
        });

        // Act
        var engineTeam = _mapper.ToEngineTeam(team);

        // Assert
        engineTeam.Players.Should().HaveCount(1);
        var player = engineTeam.Players[0];
        player.FirstName.Should().Be("John");
        player.LastName.Should().Be("Quarterback");
        player.Position.Should().Be(Gridiron.Engine.Domain.Positions.QB);
        player.Number.Should().Be(12);
        player.Speed.Should().Be(75);
        player.Passing.Should().Be(90);
    }

    [Fact]
    public void ToEntityTeam_MapsBasicProperties()
    {
        // Arrange
        var engineTeam = new Gridiron.Engine.Domain.Team
        {
            Name = "Engine Team",
            City = "Engine City",
            Wins = 5,
            Losses = 2
        };

        // Act
        var entityTeam = _mapper.ToEntityTeam(engineTeam);

        // Assert
        entityTeam.Name.Should().Be("Engine Team");
        entityTeam.City.Should().Be("Engine City");
        entityTeam.Wins.Should().Be(5);
        entityTeam.Losses.Should().Be(2);
        entityTeam.Id.Should().Be(0); // Not mapped
        entityTeam.IsDeleted.Should().BeFalse(); // Not mapped, defaults to false
    }

    [Fact]
    public void UpdateTeamEntity_PreservesIdAndSoftDelete()
    {
        // Arrange
        var entityTeam = new Team
        {
            Id = 42,
            Name = "Original Name",
            DivisionId = 5,
            IsDeleted = false,
            Wins = 0
        };

        var engineTeam = new Gridiron.Engine.Domain.Team
        {
            Name = "Updated Name",
            Wins = 10
        };

        // Act
        _mapper.UpdateTeamEntity(engineTeam, entityTeam);

        // Assert
        entityTeam.Id.Should().Be(42); // Preserved
        entityTeam.DivisionId.Should().Be(5); // Preserved
        entityTeam.IsDeleted.Should().BeFalse(); // Preserved
        entityTeam.Name.Should().Be("Updated Name"); // Updated
        entityTeam.Wins.Should().Be(10); // Updated
    }

    [Fact]
    public void ToEnginePlayer_MapsAllSkills()
    {
        // Arrange
        var player = new Player
        {
            FirstName = "Test",
            LastName = "Player",
            Position = Positions.WR,
            Speed = 95,
            Strength = 70,
            Agility = 90,
            Awareness = 85,
            Passing = 30,
            Catching = 92,
            Rushing = 60,
            Blocking = 50,
            Tackling = 40,
            Coverage = 35,
            Kicking = 20,
            Health = 100,
            Morale = 80,
            Discipline = 75,
            Potential = 88,
            Progression = 70
        };

        // Act
        var enginePlayer = _mapper.ToEnginePlayer(player);

        // Assert
        enginePlayer.Speed.Should().Be(95);
        enginePlayer.Catching.Should().Be(92);
        enginePlayer.Agility.Should().Be(90);
        enginePlayer.Potential.Should().Be(88);
        enginePlayer.Morale.Should().Be(80);
        enginePlayer.Discipline.Should().Be(75);
    }

    [Fact]
    public void PositionEnum_MapsCorrectly()
    {
        // Test a few key positions
        _mapper.ToEnginePosition(Positions.QB).Should().Be(Gridiron.Engine.Domain.Positions.QB);
        _mapper.ToEnginePosition(Positions.RB).Should().Be(Gridiron.Engine.Domain.Positions.RB);
        _mapper.ToEnginePosition(Positions.WR).Should().Be(Gridiron.Engine.Domain.Positions.WR);
        _mapper.ToEnginePosition(Positions.K).Should().Be(Gridiron.Engine.Domain.Positions.K);

        _mapper.ToEntityPosition(Gridiron.Engine.Domain.Positions.QB).Should().Be(Positions.QB);
        _mapper.ToEntityPosition(Gridiron.Engine.Domain.Positions.LB).Should().Be(Positions.LB);
    }
}

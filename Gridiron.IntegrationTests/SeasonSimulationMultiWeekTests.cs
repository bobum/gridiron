using DomainObjects;
using FluentAssertions;
using GameManagement.Services;
using Gridiron.WebApi.Controllers;
using Gridiron.WebApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Xunit;
using DataAccessLayer.Repositories;

namespace Gridiron.IntegrationTests;

[Collection("Database Collection")]
public class SeasonSimulationMultiWeekTests : IClassFixture<DatabaseTestFixture>
{
    private readonly DatabaseTestFixture _fixture;

    public SeasonSimulationMultiWeekTests(DatabaseTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task SimulateAndRevertMultipleWeeks_ShouldWorkCorrectly()
    {
        // Arrange
        using var scope = _fixture.ServiceProvider.CreateScope();
        var serviceProvider = scope.ServiceProvider;
        
        // 1. Setup League Hierarchy
        var leagueRepo = serviceProvider.GetRequiredService<ILeagueRepository>();
        var league = new League { Name = "Multi-Week Test League", IsActive = true };
        await leagueRepo.AddAsync(league);

        var conferenceRepo = serviceProvider.GetRequiredService<IConferenceRepository>();
        var conference = new Conference { Name = "Test Conf", LeagueId = league.Id };
        await conferenceRepo.AddAsync(conference);

        var divisionRepo = serviceProvider.GetRequiredService<IDivisionRepository>();
        var division = new Division { Name = "Test Div", ConferenceId = conference.Id };
        await divisionRepo.AddAsync(division);

        // 2. Setup Teams
        var teamRepo = serviceProvider.GetRequiredService<ITeamRepository>();
        var teamBuilder = serviceProvider.GetRequiredService<ITeamBuilderService>();
        
        var team1 = new Team { Name = "Team One", DivisionId = division.Id }; 
        var team2 = new Team { Name = "Team Two", DivisionId = division.Id };
        await teamRepo.AddAsync(team1);
        await teamRepo.AddAsync(team2);

        // Populate Rosters
        // Note: We need to ensure we have enough players for multiple games/injuries if that's a factor,
        // but primarily the engine needs valid depth charts.
        // Using different seeds to ensure different player stats
        team1 = teamBuilder.PopulateTeamRoster(team1, seed: 111);
        team2 = teamBuilder.PopulateTeamRoster(team2, seed: 222);
        
        // Ensure depth charts are valid and saved
        // The PopulateTeamRoster method already calls AssignDepthCharts, but we need to make sure
        // the players are actually saved to the context so the engine can retrieve them.
        // The issue might be that PopulateTeamRoster adds players to the team.Players list,
        // but they aren't tracked by the context yet if we just call UpdateAsync on the team.
        // We need to make sure the players are added to the context.
        
        await teamRepo.UpdateAsync(team1);
        await teamRepo.UpdateAsync(team2);
        await serviceProvider.GetRequiredService<DataAccessLayer.GridironDbContext>().SaveChangesAsync();

        // Re-fetch teams with players to ensure everything is correctly associated before simulation
        // This is a sanity check and helps if the context state was weird
        var teamRepoCheck = serviceProvider.GetRequiredService<ITeamRepository>();
        var t1Check = await teamRepoCheck.GetByIdWithPlayersAsync(team1.Id);
        var t2Check = await teamRepoCheck.GetByIdWithPlayersAsync(team2.Id);
        
        // Re-assign depth charts just in case
        teamBuilder.AssignDepthCharts(t1Check);
        teamBuilder.AssignDepthCharts(t2Check);
        await teamRepoCheck.UpdateAsync(t1Check);
        await teamRepoCheck.UpdateAsync(t2Check);
        await serviceProvider.GetRequiredService<DataAccessLayer.GridironDbContext>().SaveChangesAsync();

        // 3. Setup Season with 3 Weeks
        var seasonRepo = serviceProvider.GetRequiredService<ISeasonRepository>();
        var season = new Season { LeagueId = league.Id, Year = 2025, CurrentWeek = 1, Phase = SeasonPhase.RegularSeason };
        await seasonRepo.AddAsync(season);

        // Week 1
        var week1 = new SeasonWeek { SeasonId = season.Id, WeekNumber = 1, Status = WeekStatus.Scheduled, Phase = SeasonPhase.RegularSeason };
        season.Weeks.Add(week1);
        var game1 = new Game { HomeTeamId = team1.Id, AwayTeamId = team2.Id, SeasonWeek = week1, IsComplete = false };
        week1.Games.Add(game1);

        // Week 2
        var week2 = new SeasonWeek { SeasonId = season.Id, WeekNumber = 2, Status = WeekStatus.Scheduled, Phase = SeasonPhase.RegularSeason };
        season.Weeks.Add(week2);
        var game2 = new Game { HomeTeamId = team2.Id, AwayTeamId = team1.Id, SeasonWeek = week2, IsComplete = false };
        week2.Games.Add(game2);

        // Week 3 (Empty, just to advance into)
        var week3 = new SeasonWeek { SeasonId = season.Id, WeekNumber = 3, Status = WeekStatus.Scheduled, Phase = SeasonPhase.RegularSeason };
        season.Weeks.Add(week3);

        await seasonRepo.UpdateAsync(season);

        // 4. Setup Controller
        var userRepo = serviceProvider.GetRequiredService<IUserRepository>();
        var commissionerId = "commish-multi-id";
        var user = new User 
        { 
            AzureAdObjectId = commissionerId,
            Email = "commish2@example.com",
            DisplayName = "Commissioner 2",
            LeagueRoles = new List<UserLeagueRole> { new UserLeagueRole { LeagueId = league.Id, Role = UserRole.Commissioner } }
        };
        await userRepo.AddAsync(user);

        var seasonController = new SeasonsController(
            serviceProvider.GetRequiredService<ISeasonRepository>(),
            serviceProvider.GetRequiredService<ILeagueRepository>(),
            serviceProvider.GetRequiredService<IScheduleGeneratorService>(),
            serviceProvider.GetRequiredService<ISeasonSimulationService>(),
            serviceProvider.GetRequiredService<IGridironAuthorizationService>(),
            serviceProvider.GetRequiredService<ILogger<SeasonsController>>()
        );

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim("http://schemas.microsoft.com/identity/claims/objectidentifier", commissionerId),
            new Claim("oid", commissionerId)
        }, "mock"));

        seasonController.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = claimsPrincipal }
        };

        var dbContext = serviceProvider.GetRequiredService<DataAccessLayer.GridironDbContext>();

        // --- ACT & ASSERT ---

        // 1. Advance Week 1 -> 2
        var result1 = await seasonController.AdvanceWeek(season.Id);
        var ok1 = result1.Result as OkObjectResult;
        ok1.Should().NotBeNull("Week 1 advance should succeed");
        
        dbContext.ChangeTracker.Clear();
        var s1 = await seasonRepo.GetByIdWithWeeksAndGamesAsync(season.Id);
        s1.CurrentWeek.Should().Be(2, "Should be in Week 2");
        s1.Weeks.First(w => w.WeekNumber == 1).Status.Should().Be(WeekStatus.Completed);

        // 2. Advance Week 2 -> 3
        var result2 = await seasonController.AdvanceWeek(season.Id);
        
        if (result2.Result is not OkObjectResult)
        {
            var errorResult = result2.Result as ObjectResult;
            var error = errorResult?.Value;
            Assert.Fail($"Week 2 Advance failed. Result type: {result2.Result?.GetType().Name}. Error: {error}");
        }

        var ok2 = result2.Result as OkObjectResult;
        ok2.Should().NotBeNull("Week 2 advance should succeed");

        dbContext.ChangeTracker.Clear();
        var s2 = await seasonRepo.GetByIdWithWeeksAndGamesAsync(season.Id);
        s2.CurrentWeek.Should().Be(3, "Should be in Week 3");
        s2.Weeks.First(w => w.WeekNumber == 2).Status.Should().Be(WeekStatus.Completed);

        // 3. Revert Week 2 (Back to Week 2)
        // Revert logic: If CurrentWeek is 3, we revert the "last completed week", which is Week 2.
        // After revert, CurrentWeek should be 2, and Week 2 should be Scheduled.
        var revert1 = await seasonController.RevertWeek(season.Id);
        var revOk1 = revert1.Result as OkObjectResult;
        revOk1.Should().NotBeNull("First revert should succeed");

        dbContext.ChangeTracker.Clear();
        var r1 = await seasonRepo.GetByIdWithWeeksAndGamesAsync(season.Id);
        r1.CurrentWeek.Should().Be(2, "Should be back in Week 2");
        r1.Weeks.First(w => w.WeekNumber == 2).Status.Should().Be(WeekStatus.Scheduled);
        r1.Weeks.First(w => w.WeekNumber == 1).Status.Should().Be(WeekStatus.Completed); // Week 1 still done

        // 4. Revert Week 1 (Back to Week 1)
        // Revert logic: If CurrentWeek is 2, we revert Week 1.
        // After revert, CurrentWeek should be 1, and Week 1 should be Scheduled.
        var revert2 = await seasonController.RevertWeek(season.Id);
        var revOk2 = revert2.Result as OkObjectResult;
        revOk2.Should().NotBeNull("Second revert should succeed");

        dbContext.ChangeTracker.Clear();
        var r2 = await seasonRepo.GetByIdWithWeeksAndGamesAsync(season.Id);
        r2.CurrentWeek.Should().Be(1, "Should be back in Week 1");
        r2.Weeks.First(w => w.WeekNumber == 1).Status.Should().Be(WeekStatus.Scheduled);
        r2.Weeks.First(w => w.WeekNumber == 1).Games.First().IsComplete.Should().BeFalse();
    }
}

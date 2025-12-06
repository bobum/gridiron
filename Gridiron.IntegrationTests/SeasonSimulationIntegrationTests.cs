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
public class SeasonSimulationIntegrationTests : IClassFixture<DatabaseTestFixture>
{
    private readonly DatabaseTestFixture _fixture;

    public SeasonSimulationIntegrationTests(DatabaseTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task SimulateAndRevertWeek_ShouldUpdateAndResetStateCorrectly()
    {
        // Arrange
        using var scope = _fixture.ServiceProvider.CreateScope();
        var serviceProvider = scope.ServiceProvider;
        
        // 1. Create League and Season FIRST so we have IDs for the user role
        var leagueRepo = serviceProvider.GetRequiredService<ILeagueRepository>();
        var league = new League { Name = "Simulation Test League", IsActive = true };
        await leagueRepo.AddAsync(league);

        var seasonRepo = serviceProvider.GetRequiredService<ISeasonRepository>();
        var season = new Season { LeagueId = league.Id, Year = 2025, CurrentWeek = 1, Phase = SeasonPhase.RegularSeason };
        await seasonRepo.AddAsync(season);

        // 2. Create User and Assign Commissioner Role
        var userRepo = serviceProvider.GetRequiredService<IUserRepository>();
        var commissionerId = "commissioner-id";
        var user = new User 
        { 
            AzureAdObjectId = commissionerId,
            Email = "commish@example.com",
            DisplayName = "Commissioner",
            LeagueRoles = new List<UserLeagueRole>
            {
                new UserLeagueRole 
                { 
                    LeagueId = league.Id, 
                    Role = UserRole.Commissioner 
                }
            }
        };
        await userRepo.AddAsync(user);

        // 3. Setup Controller with Real Services
        var seasonController = new SeasonsController(
            serviceProvider.GetRequiredService<ISeasonRepository>(),
            serviceProvider.GetRequiredService<ILeagueRepository>(),
            serviceProvider.GetRequiredService<IScheduleGeneratorService>(),
            serviceProvider.GetRequiredService<ISeasonSimulationService>(),
            serviceProvider.GetRequiredService<IGridironAuthorizationService>(),
            serviceProvider.GetRequiredService<ILogger<SeasonsController>>()
        );

        // Setup Controller Context with User Claims
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim("http://schemas.microsoft.com/identity/claims/objectidentifier", commissionerId),
            new Claim("oid", commissionerId) // Add both claim types to be safe
        }, "mock"));

        seasonController.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = claimsPrincipal }
        };

        // 4. Create Teams
        var conferenceRepo = serviceProvider.GetRequiredService<IConferenceRepository>();
        var divisionRepo = serviceProvider.GetRequiredService<IDivisionRepository>();
        var teamRepo = serviceProvider.GetRequiredService<ITeamRepository>();
        var teamBuilder = serviceProvider.GetRequiredService<ITeamBuilderService>();
        
        var conference = new Conference { Name = "Test Conference", LeagueId = league.Id };
        await conferenceRepo.AddAsync(conference);

        var division = new Division { Name = "Test Division", ConferenceId = conference.Id };
        await divisionRepo.AddAsync(division);

        var homeTeam = new Team { Name = "Home Team", DivisionId = division.Id }; 
        var awayTeam = new Team { Name = "Away Team", DivisionId = division.Id };
        await teamRepo.AddAsync(homeTeam);
        await teamRepo.AddAsync(awayTeam);

        // Populate Rosters (Required for simulation)
        homeTeam = teamBuilder.PopulateTeamRoster(homeTeam, seed: 12345);
        awayTeam = teamBuilder.PopulateTeamRoster(awayTeam, seed: 67890);
        
        await teamRepo.UpdateAsync(homeTeam);
        await teamRepo.UpdateAsync(awayTeam);
        await serviceProvider.GetRequiredService<DataAccessLayer.GridironDbContext>().SaveChangesAsync();

        // 5. Create Schedule (Week 1 Game)
        var week = new SeasonWeek { SeasonId = season.Id, WeekNumber = 1, Status = WeekStatus.Scheduled, Phase = SeasonPhase.RegularSeason };
        season.Weeks.Add(week);
        
        // Create Week 2 (Empty) so we can advance to it
        var week2 = new SeasonWeek { SeasonId = season.Id, WeekNumber = 2, Status = WeekStatus.Scheduled, Phase = SeasonPhase.RegularSeason };
        season.Weeks.Add(week2);
        
        var game = new Game 
        { 
            HomeTeamId = homeTeam.Id, 
            AwayTeamId = awayTeam.Id, 
            SeasonWeek = week,
            IsComplete = false 
        };
        week.Games.Add(game);
        await seasonRepo.UpdateAsync(season);

        // Act 1: Advance Week
        var advanceResult = await seasonController.AdvanceWeek(season.Id);
        
        // Assert 1: Verify Simulation
        if (advanceResult.Result is not OkObjectResult)
        {
            var errorResult = advanceResult.Result as ObjectResult;
            var error = errorResult?.Value;
            // This will fail but give us the error message in the test output
            Assert.Fail($"AdvanceWeek failed. Result type: {advanceResult.Result?.GetType().Name}. Error: {error}");
        }

        var okResult = advanceResult.Result as OkObjectResult;
        okResult.Should().NotBeNull();
        var simResult = okResult!.Value as SeasonSimulationResult;
        simResult!.Success.Should().BeTrue();
        simResult.GamesSimulated.Should().Be(1);
        
        // Verify DB State after Advance
        // Need to detach or reload to get fresh state from DB
        var dbContext = serviceProvider.GetRequiredService<DataAccessLayer.GridironDbContext>();
        dbContext.ChangeTracker.Clear();

        var updatedSeason = await seasonRepo.GetByIdWithWeeksAndGamesAsync(season.Id);
        updatedSeason!.CurrentWeek.Should().Be(2); // Should have advanced to week 2
        var updatedWeek1 = updatedSeason.Weeks.First(w => w.WeekNumber == 1);
        updatedWeek1.Status.Should().Be(WeekStatus.Completed);
        updatedWeek1.Games.First().IsComplete.Should().BeTrue();
        
        // Verify PlayByPlay was created
        var playByPlayRepo = serviceProvider.GetRequiredService<IPlayByPlayRepository>();
        var playByPlay = await playByPlayRepo.GetByGameIdAsync(updatedWeek1.Games.First().Id);
        playByPlay.Should().NotBeNull("PlayByPlay record should be created");
        playByPlay!.PlaysJson.Should().NotBeNullOrEmpty();
        // Log is currently empty because Gridiron.Engine v0.1.0 does not write to the passed ILogger.
        // We assert NotBeNull to ensure the property is present, but allow empty string until engine is updated.
        playByPlay.PlayByPlayLog.Should().NotBeNull(); 

        // Act 2: Revert Week
        var revertResult = await seasonController.RevertWeek(season.Id);

        // Assert 2: Verify Revert
        var revertOkResult = revertResult.Result as OkObjectResult;
        revertOkResult.Should().NotBeNull();
        
        // Verify DB State after Revert
        dbContext.ChangeTracker.Clear();
        var revertedSeason = await seasonRepo.GetByIdWithWeeksAndGamesAsync(season.Id);
        revertedSeason!.CurrentWeek.Should().Be(1); // Should be back to week 1
        var revertedWeek1 = revertedSeason.Weeks.First(w => w.WeekNumber == 1);
        revertedWeek1.Status.Should().Be(WeekStatus.Scheduled);
        
        var revertedGame = revertedWeek1.Games.First();
        revertedGame.IsComplete.Should().BeFalse();
        revertedGame.HomeScore.Should().Be(0);
        revertedGame.AwayScore.Should().Be(0);
        revertedGame.PlayedAt.Should().BeNull();

        // Verify PlayByPlay was deleted
        var deletedPlayByPlay = await playByPlayRepo.GetByGameIdAsync(revertedGame.Id);
        deletedPlayByPlay.Should().BeNull("PlayByPlay record should be deleted after revert");
    }
}

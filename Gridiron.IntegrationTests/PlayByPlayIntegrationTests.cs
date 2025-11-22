using DataAccessLayer.Repositories;
using DomainObjects;
using FluentAssertions;
using GameManagement.Services;
using Gridiron.WebApi.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace Gridiron.IntegrationTests;

/// <summary>
/// Comprehensive end-to-end integration test for the entire PlayByPlay feature
/// Tests the complete workflow from league creation through game simulation to PlayByPlay retrieval
/// </summary>
[Trait("Category", "Integration")]
public class PlayByPlayIntegrationTests : IClassFixture<DatabaseTestFixture>
{
    private readonly DatabaseTestFixture _fixture;
    private readonly ITestOutputHelper _output;

    public PlayByPlayIntegrationTests(DatabaseTestFixture fixture, ITestOutputHelper output)
    {
        _fixture = fixture;
        _output = output;
    }

    [Fact]
    public async Task CompleteWorkflow_CreateLeague_PopulateTeams_SimulateGame_RetrievePlayByPlay_Success()
    {
        // This test verifies the entire system works end-to-end:
        // 1. Create a league with hierarchical structure (2 conferences, 4 divisions each, 4 teams per division = 32 teams)
        // 2. Populate all 32 teams with 53 players each (1,696 total players)
        // 3. Pick two teams from different divisions
        // 4. Simulate a complete game between them
        // 5. Verify game is persisted to database with correct data
        // 6. Verify PlayByPlay is persisted with PlaysJson and PlayByPlayLog
        // 7. Retrieve game from database
        // 8. Retrieve PlayByPlay from database
        // 9. Verify PlayByPlayLog contains actual play-by-play (no diagnostic state transitions)

        // ==========================================
        // STEP 1: Create League with Structure
        // ==========================================
        var leagueBuilder = _fixture.ServiceProvider.GetRequiredService<ILeagueBuilderService>();
        var leagueRepository = _fixture.ServiceProvider.GetRequiredService<ILeagueRepository>();

        var league = leagueBuilder.CreateLeague(
            leagueName: "Test Football League",
            numberOfConferences: 2,
            divisionsPerConference: 4,
            teamsPerDivision: 4
        );

        league.Should().NotBeNull();
        league.Conferences.Should().HaveCount(2);
        league.Conferences.SelectMany(c => c.Divisions).Should().HaveCount(8); // 2 * 4
        var allTeams = league.Conferences
            .SelectMany(c => c.Divisions)
            .SelectMany(d => d.Teams)
            .ToList();
        allTeams.Should().HaveCount(32); // 2 * 4 * 4

        // Persist the league to database
        var persistedLeague = await leagueRepository.AddAsync(league);
        await leagueRepository.SaveChangesAsync();

        // Verify league persisted
        var retrievedLeague = await leagueRepository.GetByIdAsync(persistedLeague.Id);
        retrievedLeague.Should().NotBeNull();

        // ==========================================
        // STEP 2: Populate All Teams with Players
        // ==========================================
        var populatedLeague = leagueBuilder.PopulateLeagueRosters(persistedLeague, seed: 12345);

        // Update league in database with populated rosters
        await leagueRepository.UpdateAsync(populatedLeague);
        await leagueRepository.SaveChangesAsync();

        // Retrieve league with all teams and players to verify
        var retrievedPopulatedLeague = await leagueRepository.GetByIdWithFullStructureAsync(populatedLeague.Id);
        retrievedPopulatedLeague.Should().NotBeNull();

        var allPopulatedTeams = retrievedPopulatedLeague!.Conferences
            .SelectMany(c => c.Divisions)
            .SelectMany(d => d.Teams)
            .ToList();

        allPopulatedTeams.Should().HaveCount(32);

        // Verify each team has 53 players
        var teamRepository = _fixture.ServiceProvider.GetRequiredService<ITeamRepository>();
        foreach (var team in allPopulatedTeams)
        {
            var teamWithPlayers = await teamRepository.GetByIdWithPlayersAsync(team.Id);
            teamWithPlayers.Should().NotBeNull();
            teamWithPlayers!.Players.Should().HaveCount(53,
                $"because team '{teamWithPlayers.City} {teamWithPlayers.Name}' should have 53 players");
        }

        // ==========================================
        // STEP 3: Pick Two Teams from Different Divisions
        // ==========================================
        var firstConferenceFirstDivisionFirstTeam = retrievedPopulatedLeague.Conferences[0].Divisions[0].Teams[0];
        var secondConferenceSecondDivisionThirdTeam = retrievedPopulatedLeague.Conferences[1].Divisions[1].Teams[2];

        firstConferenceFirstDivisionFirstTeam.Should().NotBeNull();
        secondConferenceSecondDivisionThirdTeam.Should().NotBeNull();
        firstConferenceFirstDivisionFirstTeam.Id.Should().NotBe(secondConferenceSecondDivisionThirdTeam.Id);

        var homeTeamId = firstConferenceFirstDivisionFirstTeam.Id;
        var awayTeamId = secondConferenceSecondDivisionThirdTeam.Id;

        // ==========================================
        // STEP 4: Simulate Complete Game
        // ==========================================
        var gameSimulationService = _fixture.ServiceProvider.GetRequiredService<IGameSimulationService>();

        var simulatedGame = await gameSimulationService.SimulateGameAsync(
            homeTeamId: homeTeamId,
            awayTeamId: awayTeamId,
            randomSeed: 99999 // Reproducible game
        );

        simulatedGame.Should().NotBeNull();
        simulatedGame.Id.Should().BeGreaterThan(0, "because game should be persisted with an ID");
        simulatedGame.HomeTeamId.Should().Be(homeTeamId);
        simulatedGame.AwayTeamId.Should().Be(awayTeamId);
        simulatedGame.RandomSeed.Should().Be(99999);

        // Verify scores were set (game was actually simulated)
        var totalScore = simulatedGame.HomeScore + simulatedGame.AwayScore;
        totalScore.Should().BeGreaterThan(0, "because the game should have produced some scoring");

        // Verify plays were created
        simulatedGame.Plays.Should().NotBeNull();
        simulatedGame.Plays.Should().NotBeEmpty("because game simulation should create plays");
        simulatedGame.Plays.Count.Should().BeGreaterThan(50, "because a full game should have many plays");

        // ==========================================
        // STEP 5: Verify Game Persisted to Database
        // ==========================================
        var gameRepository = _fixture.ServiceProvider.GetRequiredService<IGameRepository>();
        var retrievedGame = await gameRepository.GetByIdWithTeamsAsync(simulatedGame.Id);

        retrievedGame.Should().NotBeNull();
        retrievedGame!.Id.Should().Be(simulatedGame.Id);
        retrievedGame.HomeTeamId.Should().Be(homeTeamId);
        retrievedGame.AwayTeamId.Should().Be(awayTeamId);
        retrievedGame.HomeScore.Should().Be(simulatedGame.HomeScore);
        retrievedGame.AwayScore.Should().Be(simulatedGame.AwayScore);
        retrievedGame.RandomSeed.Should().Be(99999);
        retrievedGame.HomeTeam.Should().NotBeNull();
        retrievedGame.AwayTeam.Should().NotBeNull();

        // ==========================================
        // STEP 6: Verify PlayByPlay Persisted to Database
        // ==========================================
        var playByPlayRepository = _fixture.ServiceProvider.GetRequiredService<IPlayByPlayRepository>();
        var playByPlayExists = await playByPlayRepository.ExistsForGameAsync(simulatedGame.Id);

        playByPlayExists.Should().BeTrue("because PlayByPlay should be created for the game");

        // ==========================================
        // STEP 7: Retrieve PlayByPlay from Database
        // ==========================================
        var retrievedPlayByPlay = await playByPlayRepository.GetByGameIdAsync(simulatedGame.Id);

        retrievedPlayByPlay.Should().NotBeNull();
        retrievedPlayByPlay!.GameId.Should().Be(simulatedGame.Id);
        retrievedPlayByPlay.Id.Should().BeGreaterThan(0);
        retrievedPlayByPlay.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(5));

        // ==========================================
        // STEP 8: Verify PlaysJson is Valid
        // ==========================================
        retrievedPlayByPlay.PlaysJson.Should().NotBeNullOrEmpty("because plays should be serialized to JSON");
        retrievedPlayByPlay.PlaysJson.Should().Contain("{", "because JSON should contain objects");
        retrievedPlayByPlay.PlaysJson.Should().Contain("}", "because JSON should contain objects");
        retrievedPlayByPlay.PlaysJson.Should().Contain("playType", "because plays should have playType field");

        // ==========================================
        // STEP 9: Verify PlayByPlayLog Contains Actual Play-by-Play
        // ==========================================
        retrievedPlayByPlay.PlayByPlayLog.Should().NotBeNullOrEmpty("because play-by-play log should be captured");

        // Log should be comprehensive (full game commentary)
        retrievedPlayByPlay.PlayByPlayLog.Length.Should().BeGreaterThan(1000,
            "because a full game generates extensive play-by-play commentary");

        // Verify play-by-play content (actual game commentary)
        retrievedPlayByPlay.PlayByPlayLog.Should().Contain("Good snap",
            "because game engine logs snap events");

        // Verify down and distance patterns (e.g., "1st & 10", "2nd & 5")
        retrievedPlayByPlay.PlayByPlayLog.Should().MatchRegex(@"\d+(st|nd|rd|th) & \d+",
            "because game logs down and distance");

        // Verify scoring events if game had touchdowns
        if (totalScore >= 6)
        {
            retrievedPlayByPlay.PlayByPlayLog.Should().ContainAny(
                "TOUCHDOWN",
                "FIELD GOAL",
                "because game with scoring should log scoring plays");
        }

        // ==========================================
        // STEP 10: Verify Player Stats Accumulated During Game (Deterministic with seed 99999)
        // ==========================================
        var homeTeamWithPlayers = await teamRepository.GetByIdWithPlayersAsync(homeTeamId);
        var awayTeamWithPlayers = await teamRepository.GetByIdWithPlayersAsync(awayTeamId);

        homeTeamWithPlayers.Should().NotBeNull();
        awayTeamWithPlayers.Should().NotBeNull();

        // With deterministic seeding, we should get EXACT same stats every time
        // This verifies both stat accumulation AND deterministic simulation

        // Home Team QB - Kenneth Henderson
        var homeQB = homeTeamWithPlayers!.Players.First(p => p.FirstName == "Kenneth" && p.LastName == "Henderson");
        homeQB.Stats[DomainObjects.StatTypes.PlayerStatType.PassingYards].Should().Be(192);
        homeQB.Stats[DomainObjects.StatTypes.PlayerStatType.PassingAttempts].Should().Be(19);
        homeQB.Stats[DomainObjects.StatTypes.PlayerStatType.PassingCompletions].Should().Be(11);
        homeQB.Stats[DomainObjects.StatTypes.PlayerStatType.PassingTouchdowns].Should().Be(3);

        // Home Team RB - Brian Powell
        var homeRB = homeTeamWithPlayers.Players.First(p => p.FirstName == "Brian" && p.LastName == "Powell");
        homeRB.Stats[DomainObjects.StatTypes.PlayerStatType.RushingYards].Should().Be(137);
        homeRB.Stats[DomainObjects.StatTypes.PlayerStatType.RushingAttempts].Should().Be(17);
        homeRB.Stats[DomainObjects.StatTypes.PlayerStatType.RushingTouchdowns].Should().Be(2);

        // Home Team WR - Deandre James
        var homeWR = homeTeamWithPlayers.Players.First(p => p.FirstName == "Deandre" && p.LastName == "James");
        homeWR.Stats[DomainObjects.StatTypes.PlayerStatType.Receptions].Should().Be(5);
        homeWR.Stats[DomainObjects.StatTypes.PlayerStatType.ReceivingYards].Should().Be(56);
        homeWR.Stats[DomainObjects.StatTypes.PlayerStatType.ReceivingTouchdowns].Should().Be(2);

        // Away Team QB - Henry Graham
        var awayQB = awayTeamWithPlayers!.Players.First(p => p.FirstName == "Henry" && p.LastName == "Graham");
        awayQB.Stats[DomainObjects.StatTypes.PlayerStatType.PassingYards].Should().Be(319);
        awayQB.Stats[DomainObjects.StatTypes.PlayerStatType.PassingAttempts].Should().Be(30);
        awayQB.Stats[DomainObjects.StatTypes.PlayerStatType.PassingCompletions].Should().Be(17);
        awayQB.Stats[DomainObjects.StatTypes.PlayerStatType.PassingTouchdowns].Should().Be(2);

        // Away Team RB - Jimmy Kelly
        var awayRB = awayTeamWithPlayers.Players.First(p => p.FirstName == "Jimmy" && p.LastName == "Kelly");
        awayRB.Stats[DomainObjects.StatTypes.PlayerStatType.RushingYards].Should().Be(235);
        awayRB.Stats[DomainObjects.StatTypes.PlayerStatType.RushingAttempts].Should().Be(40);
        awayRB.Stats[DomainObjects.StatTypes.PlayerStatType.RushingTouchdowns].Should().Be(4);

        // Away Team WR - Bryan Ward
        var awayWR = awayTeamWithPlayers.Players.First(p => p.FirstName == "Bryan" && p.LastName == "Ward");
        awayWR.Stats[DomainObjects.StatTypes.PlayerStatType.Receptions].Should().Be(6);
        awayWR.Stats[DomainObjects.StatTypes.PlayerStatType.ReceivingYards].Should().Be(78);
        awayWR.Stats[DomainObjects.StatTypes.PlayerStatType.ReceivingTouchdowns].Should().Be(1);

        // ==========================================
        // STEP 11: Verify State Transitions Filtered Out
        // ==========================================
        retrievedPlayByPlay.PlayByPlayLog.Should().NotContain("State transition:",
            "because diagnostic state transition messages should be filtered from play-by-play log");

        // ==========================================
        // TEST COMPLETE - All Integration Points Verified
        // ==========================================
        // This test has verified:
        // ✓ League creation with hierarchical structure
        // ✓ Team population with 53 players each
        // ✓ Database persistence of league, conferences, divisions, teams, players
        // ✓ Game simulation between two teams
        // ✓ Game persistence with correct teams and scores
        // ✓ PlayByPlay creation and persistence
        // ✓ PlaysJson serialization
        // ✓ PlayByPlayLog capture from game engine logging
        // ✓ Player statistics accumulation during game simulation
        // ✓ Filtering of diagnostic state transition messages
        // ✓ Complete end-to-end workflow functions correctly
    }
}

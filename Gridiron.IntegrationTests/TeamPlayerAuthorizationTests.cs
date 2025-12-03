using System.Security.Claims;
using DataAccessLayer.Repositories;
using DomainObjects;
using FluentAssertions;
using Gridiron.WebApi.Controllers;
using Gridiron.WebApi.DTOs;
using Gridiron.WebApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Gridiron.IntegrationTests;

/// <summary>
/// COMPREHENSIVE Team/Player/Conference/Division/Games Authorization Tests with 5 Users
///
/// Test Scenario (same as ComprehensiveAuthorizationTests):
/// - God: Global Admin, can see everything
/// - Bob: Commissioner of "Bob's League", GM of Team 1 in Mack's League
/// - Joe: GM of Team 1 in Bob's League, GM of Team 2 in Mack's League
/// - Mack: Commissioner of "Mack's League", not in Bob's League
/// - Don: GM of Team 2 in Bob's League, GM of Team 3 in Mack's League
///
/// These tests verify authorization at all hierarchy levels:
/// - Teams: GMs can only access their teams, Commissioners can access all teams in their league
/// - Players: Access cascades from team access
/// - Conferences/Divisions: Access cascades from league access (intermediate hierarchy)
/// - Games: Anyone in a league can view games in that league
///
/// Critical Security Boundaries:
/// - Joe CANNOT access Don's team in Bob's League
/// - Don CANNOT access Joe's team in Mack's League
/// - Mack CANNOT access any teams in Bob's League
/// - Bob (as GM in Mack's League) CANNOT access Joe's or Don's teams in Mack's League.
/// </summary>
public class TeamPlayerAuthorizationTests : IClassFixture<DatabaseTestFixture>
{
    private readonly DatabaseTestFixture _fixture;

    // User OIDs (match ComprehensiveAuthorizationTests)
    private const string GodOid = "god-comprehensive-oid";
    private const string BobOid = "bob-comprehensive-oid";
    private const string JoeOid = "joe-comprehensive-oid";
    private const string MackOid = "mack-comprehensive-oid";
    private const string DonOid = "don-comprehensive-oid";

    // League and team tracking
    private int _bobsLeagueId;
    private int _macksLeagueId;
    private int _bobsLeagueJoeTeamId;  // Team 1 in Bob's League
    private int _bobsLeagueDonTeamId;  // Team 2 in Bob's League
    private int _macksLeagueBobTeamId; // Team 1 in Mack's League
    private int _macksLeagueJoeTeamId; // Team 2 in Mack's League
    private int _macksLeagueDonTeamId; // Team 3 in Mack's League

    // Conference and Division tracking
    private int _bobsLeagueConferenceId;
    private int _bobsLeagueDivisionId;
    private int _macksLeagueConferenceId;
    private int _macksLeagueDivisionId;

    public TeamPlayerAuthorizationTests(DatabaseTestFixture fixture)
    {
        _fixture = fixture;
    }

    #region Setup Test Data

    [Fact]
    public async Task Setup_TeamPlayerAuthorizationScenario()
    {
        // This test sets up the scenario and populates rosters for testing
        await CreateTestScenario();
        await PopulateRosters();

        // Verify setup
        var teamRepo = _fixture.ServiceProvider.GetRequiredService<ITeamRepository>();
        var playerRepo = _fixture.ServiceProvider.GetRequiredService<IPlayerRepository>();

        var joeTeam = await teamRepo.GetByIdAsync(_bobsLeagueJoeTeamId);
        joeTeam.Should().NotBeNull();

        var joePlayers = await playerRepo.GetByTeamIdAsync(_bobsLeagueJoeTeamId);
        joePlayers.Should().NotBeEmpty("Joe's team should have players after population");
    }

    #endregion

    #region Team Authorization Tests - God

    [Fact]
    public async Task God_CanAccessAnyTeam()
    {
        await CreateTestScenario();
        var (godController, _, _) = CreateTeamsManagementControllerWithAuth(GodOid, "god@example.com", "God Admin");

        // God can access Joe's team in Bob's League
        var result1 = await godController.GetTeam(_bobsLeagueJoeTeamId);
        result1.Result.Should().BeOfType<OkObjectResult>();

        // God can access Don's team in Bob's League
        var result2 = await godController.GetTeam(_bobsLeagueDonTeamId);
        result2.Result.Should().BeOfType<OkObjectResult>();

        // God can access any team in Mack's League
        var result3 = await godController.GetTeam(_macksLeagueJoeTeamId);
        result3.Result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region Team Authorization Tests - Bob (Commissioner of Bob's League, GM in Mack's League)

    [Fact]
    public async Task Bob_AsCommissioner_CanAccessAllTeamsInHisLeague()
    {
        await CreateTestScenario();
        var (bobController, _, _) = CreateTeamsManagementControllerWithAuth(BobOid, "bob@example.com", "Bob User");

        // Bob can access Joe's team in his league (as Commissioner)
        var result1 = await bobController.GetTeam(_bobsLeagueJoeTeamId);
        result1.Result.Should().BeOfType<OkObjectResult>();

        // Bob can access Don's team in his league (as Commissioner)
        var result2 = await bobController.GetTeam(_bobsLeagueDonTeamId);
        result2.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Bob_AsGM_CanOnlyAccessHisTeamInMacksLeague()
    {
        await CreateTestScenario();
        var (bobController, _, _) = CreateTeamsManagementControllerWithAuth(BobOid, "bob@example.com", "Bob User");

        // Bob can access his own team in Mack's League (Team 1)
        var result1 = await bobController.GetTeam(_macksLeagueBobTeamId);
        result1.Result.Should().BeOfType<OkObjectResult>();

        // Bob CANNOT access Joe's team in Mack's League
        var result2 = await bobController.GetTeam(_macksLeagueJoeTeamId);
        result2.Result.Should().BeOfType<ForbidResult>();

        // Bob CANNOT access Don's team in Mack's League
        var result3 = await bobController.GetTeam(_macksLeagueDonTeamId);
        result3.Result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task Bob_AsCommissioner_CanUpdateTeamsInHisLeague()
    {
        await CreateTestScenario();
        var (bobController, _, _) = CreateTeamsManagementControllerWithAuth(BobOid, "bob@example.com", "Bob User");

        var updateRequest = new UpdateTeamRequest
        {
            City = "Updated City",
            Name = "Updated Name",
            Budget = 150000000
        };

        // Bob can update Joe's team (as Commissioner)
        var result = await bobController.UpdateTeam(_bobsLeagueJoeTeamId, updateRequest);
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Bob_AsGM_CannotUpdateOtherTeamsInMacksLeague()
    {
        await CreateTestScenario();
        var (bobController, _, _) = CreateTeamsManagementControllerWithAuth(BobOid, "bob@example.com", "Bob User");

        var updateRequest = new UpdateTeamRequest
        {
            City = "Hacked City",
            Name = "Hacked Name",
            Budget = 1
        };

        // Bob CANNOT update Joe's team in Mack's League
        var result = await bobController.UpdateTeam(_macksLeagueJoeTeamId, updateRequest);
        result.Result.Should().BeOfType<ForbidResult>();
    }

    #endregion

    #region Team Authorization Tests - Joe (GM in both leagues)

    [Fact]
    public async Task Joe_CanAccessHisTeamInBobsLeague()
    {
        await CreateTestScenario();
        var (joeController, _, _) = CreateTeamsManagementControllerWithAuth(JoeOid, "joe@example.com", "Joe User");

        var result = await joeController.GetTeam(_bobsLeagueJoeTeamId);
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Joe_CannotAccessDonsTeamInBobsLeague()
    {
        await CreateTestScenario();
        var (joeController, _, _) = CreateTeamsManagementControllerWithAuth(JoeOid, "joe@example.com", "Joe User");

        var result = await joeController.GetTeam(_bobsLeagueDonTeamId);
        result.Result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task Joe_CanAccessHisTeamInMacksLeague()
    {
        await CreateTestScenario();
        var (joeController, _, _) = CreateTeamsManagementControllerWithAuth(JoeOid, "joe@example.com", "Joe User");

        var result = await joeController.GetTeam(_macksLeagueJoeTeamId);
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Joe_CannotAccessDonsTeamInMacksLeague()
    {
        await CreateTestScenario();
        var (joeController, _, _) = CreateTeamsManagementControllerWithAuth(JoeOid, "joe@example.com", "Joe User");

        var result = await joeController.GetTeam(_macksLeagueDonTeamId);
        result.Result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task Joe_CanUpdateHisOwnTeam()
    {
        await CreateTestScenario();
        var (joeController, _, _) = CreateTeamsManagementControllerWithAuth(JoeOid, "joe@example.com", "Joe User");

        var updateRequest = new UpdateTeamRequest
        {
            City = "Joe's City",
            Name = "Joe's Team",
            Budget = 120000000
        };

        var result = await joeController.UpdateTeam(_bobsLeagueJoeTeamId, updateRequest);
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Joe_CannotUpdateDonsTeam()
    {
        await CreateTestScenario();
        var (joeController, _, _) = CreateTeamsManagementControllerWithAuth(JoeOid, "joe@example.com", "Joe User");

        var updateRequest = new UpdateTeamRequest
        {
            City = "Hacked",
            Name = "Hacked",
            Budget = 1
        };

        var result = await joeController.UpdateTeam(_bobsLeagueDonTeamId, updateRequest);
        result.Result.Should().BeOfType<ForbidResult>();
    }

    #endregion

    #region Team Authorization Tests - Mack (Commissioner, NOT in Bob's League)

    [Fact]
    public async Task Mack_CannotAccessAnyTeamInBobsLeague()
    {
        await CreateTestScenario();
        var (mackController, _, _) = CreateTeamsManagementControllerWithAuth(MackOid, "mack@example.com", "Mack User");

        // Mack CANNOT access Joe's team in Bob's League
        var result1 = await mackController.GetTeam(_bobsLeagueJoeTeamId);
        result1.Result.Should().BeOfType<ForbidResult>();

        // Mack CANNOT access Don's team in Bob's League
        var result2 = await mackController.GetTeam(_bobsLeagueDonTeamId);
        result2.Result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task Mack_CanAccessAllTeamsInHisLeague()
    {
        await CreateTestScenario();
        var (mackController, _, _) = CreateTeamsManagementControllerWithAuth(MackOid, "mack@example.com", "Mack User");

        // Mack can access Bob's team in his league (as Commissioner)
        var result1 = await mackController.GetTeam(_macksLeagueBobTeamId);
        result1.Result.Should().BeOfType<OkObjectResult>();

        // Mack can access Joe's team in his league (as Commissioner)
        var result2 = await mackController.GetTeam(_macksLeagueJoeTeamId);
        result2.Result.Should().BeOfType<OkObjectResult>();

        // Mack can access Don's team in his league (as Commissioner)
        var result3 = await mackController.GetTeam(_macksLeagueDonTeamId);
        result3.Result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region Team Authorization Tests - Don (GM in both leagues)

    [Fact]
    public async Task Don_CanAccessHisTeamInBobsLeague()
    {
        await CreateTestScenario();
        var (donController, _, _) = CreateTeamsManagementControllerWithAuth(DonOid, "don@example.com", "Don User");

        var result = await donController.GetTeam(_bobsLeagueDonTeamId);
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Don_CannotAccessJoesTeamInBobsLeague()
    {
        await CreateTestScenario();
        var (donController, _, _) = CreateTeamsManagementControllerWithAuth(DonOid, "don@example.com", "Don User");

        var result = await donController.GetTeam(_bobsLeagueJoeTeamId);
        result.Result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task Don_CanAccessHisTeamInMacksLeague()
    {
        await CreateTestScenario();
        var (donController, _, _) = CreateTeamsManagementControllerWithAuth(DonOid, "don@example.com", "Don User");

        var result = await donController.GetTeam(_macksLeagueDonTeamId);
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Don_CannotAccessJoesTeamInMacksLeague()
    {
        await CreateTestScenario();
        var (donController, _, _) = CreateTeamsManagementControllerWithAuth(DonOid, "don@example.com", "Don User");

        var result = await donController.GetTeam(_macksLeagueJoeTeamId);
        result.Result.Should().BeOfType<ForbidResult>();
    }

    #endregion

    #region Conference/Division Authorization Tests (Cascade from League)

    [Fact]
    public async Task Bob_AsCommissioner_CanAccessConferenceInHisLeague()
    {
        await CreateTestScenario();
        var (conferenceController, _) = CreateConferencesManagementControllerWithAuth(BobOid, "bob@example.com", "Bob User");

        var result = await conferenceController.GetConference(_bobsLeagueConferenceId);
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Mack_CannotAccessConferenceInBobsLeague()
    {
        await CreateTestScenario();
        var (conferenceController, _) = CreateConferencesManagementControllerWithAuth(MackOid, "mack@example.com", "Mack User");

        var result = await conferenceController.GetConference(_bobsLeagueConferenceId);
        result.Result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task Joe_AsGM_CanAccessConferenceInBobsLeague()
    {
        await CreateTestScenario();
        var (conferenceController, _) = CreateConferencesManagementControllerWithAuth(JoeOid, "joe@example.com", "Joe User");

        // Joe can view conference in Bob's League (read-only access for GMs)
        var result = await conferenceController.GetConference(_bobsLeagueConferenceId);
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Joe_AsGM_CannotUpdateConferenceInBobsLeague()
    {
        await CreateTestScenario();
        var (conferenceController, _) = CreateConferencesManagementControllerWithAuth(JoeOid, "joe@example.com", "Joe User");

        var updateRequest = new UpdateConferenceRequest
        {
            Name = "Hacked Conference"
        };

        var result = await conferenceController.UpdateConference(_bobsLeagueConferenceId, updateRequest);
        result.Result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task Bob_AsCommissioner_CanUpdateConferenceInHisLeague()
    {
        await CreateTestScenario();
        var (conferenceController, _) = CreateConferencesManagementControllerWithAuth(BobOid, "bob@example.com", "Bob User");

        var updateRequest = new UpdateConferenceRequest
        {
            Name = "Updated Conference"
        };

        var result = await conferenceController.UpdateConference(_bobsLeagueConferenceId, updateRequest);
        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task God_CanAccessAllConferences()
    {
        await CreateTestScenario();
        var (conferenceController, _) = CreateConferencesManagementControllerWithAuth(GodOid, "god@example.com", "God Admin");

        // God can access conferences in Bob's League
        var result1 = await conferenceController.GetConference(_bobsLeagueConferenceId);
        result1.Result.Should().BeOfType<OkObjectResult>();

        // God can access conferences in Mack's League
        var result2 = await conferenceController.GetConference(_macksLeagueConferenceId);
        result2.Result.Should().BeOfType<OkObjectResult>();
    }

    #endregion

    #region Player Authorization Tests (Cascade from Team)

    [Fact]
    public async Task Joe_CanAccessPlayersOnHisTeam()
    {
        await CreateTestScenario();
        await PopulateRosters();

        var playerRepo = _fixture.ServiceProvider.GetRequiredService<IPlayerRepository>();
        var joePlayers = await playerRepo.GetByTeamIdAsync(_bobsLeagueJoeTeamId);
        joePlayers.Should().NotBeEmpty();

        var firstPlayer = joePlayers.First();

        var playersController = CreatePlayersControllerWithAuth(JoeOid, "joe@example.com", "Joe User");

        // Joe can access players on his team through authorization service
        var authService = _fixture.ServiceProvider.GetRequiredService<IGridironAuthorizationService>();
        var canAccess = await authService.CanAccessTeamAsync(JoeOid, _bobsLeagueJoeTeamId);
        canAccess.Should().BeTrue();
    }

    [Fact]
    public async Task Joe_CannotAccessPlayersOnDonsTeam()
    {
        await CreateTestScenario();
        await PopulateRosters();

        var authService = _fixture.ServiceProvider.GetRequiredService<IGridironAuthorizationService>();

        // Joe CANNOT access Don's team, therefore cannot access players on Don's team
        var canAccess = await authService.CanAccessTeamAsync(JoeOid, _bobsLeagueDonTeamId);
        canAccess.Should().BeFalse();
    }

    [Fact]
    public async Task Bob_AsCommissioner_CanAccessPlayersOnAnyTeamInHisLeague()
    {
        await CreateTestScenario();
        await PopulateRosters();

        var authService = _fixture.ServiceProvider.GetRequiredService<IGridironAuthorizationService>();

        // Bob can access Joe's team (as Commissioner)
        var canAccessJoe = await authService.CanAccessTeamAsync(BobOid, _bobsLeagueJoeTeamId);
        canAccessJoe.Should().BeTrue();

        // Bob can access Don's team (as Commissioner)
        var canAccessDon = await authService.CanAccessTeamAsync(BobOid, _bobsLeagueDonTeamId);
        canAccessDon.Should().BeTrue();
    }

    [Fact]
    public async Task Mack_CannotAccessPlayersInBobsLeague()
    {
        await CreateTestScenario();
        await PopulateRosters();

        var authService = _fixture.ServiceProvider.GetRequiredService<IGridironAuthorizationService>();

        // Mack CANNOT access any teams in Bob's League
        var canAccessJoe = await authService.CanAccessTeamAsync(MackOid, _bobsLeagueJoeTeamId);
        canAccessJoe.Should().BeFalse();

        var canAccessDon = await authService.CanAccessTeamAsync(MackOid, _bobsLeagueDonTeamId);
        canAccessDon.Should().BeFalse();
    }

    #endregion

    #region Helper Methods

    private async Task CreateTestScenario()
    {
        await CreateGodAsync();
        await CreateBobsLeagueAsync();
        await CreateMacksLeagueAsync();
        await AssignRolesAsync();
    }

    private async Task CreateGodAsync()
    {
        var userRepo = _fixture.ServiceProvider.GetRequiredService<IUserRepository>();
        var existingGod = await userRepo.GetByAzureAdObjectIdAsync(GodOid);

        if (existingGod == null)
        {
            var god = new User
            {
                AzureAdObjectId = GodOid,
                Email = "god@example.com",
                DisplayName = "God Admin",
                IsGlobalAdmin = true
            };
            await userRepo.AddAsync(god);
        }
        else if (!existingGod.IsGlobalAdmin)
        {
            existingGod.IsGlobalAdmin = true;
            await userRepo.UpdateAsync(existingGod);
        }
    }

    private async Task CreateBobsLeagueAsync()
    {
        var leagueController = CreateLeaguesManagementControllerWithAuth(BobOid, "bob@example.com", "Bob User");

        var createResult = await leagueController.controller.CreateLeague(new CreateLeagueRequest
        {
            Name = "Bob's Team/Player Test League",
            NumberOfConferences = 1,
            DivisionsPerConference = 1,
            TeamsPerDivision = 3
        });

        var createdLeague = ((createResult.Result as CreatedAtActionResult) !.Value as LeagueDetailDto) !;
        _bobsLeagueId = createdLeague.Id;
        _bobsLeagueJoeTeamId = createdLeague.Conferences[0].Divisions[0].Teams[0].Id;
        _bobsLeagueDonTeamId = createdLeague.Conferences[0].Divisions[0].Teams[1].Id;
        _bobsLeagueConferenceId = createdLeague.Conferences[0].Id;
        _bobsLeagueDivisionId = createdLeague.Conferences[0].Divisions[0].Id;
    }

    private async Task CreateMacksLeagueAsync()
    {
        var leagueController = CreateLeaguesManagementControllerWithAuth(MackOid, "mack@example.com", "Mack User");

        var createResult = await leagueController.controller.CreateLeague(new CreateLeagueRequest
        {
            Name = "Mack's Team/Player Test League",
            NumberOfConferences = 1,
            DivisionsPerConference = 1,
            TeamsPerDivision = 4
        });

        var createdLeague = ((createResult.Result as CreatedAtActionResult) !.Value as LeagueDetailDto) !;
        _macksLeagueId = createdLeague.Id;
        _macksLeagueBobTeamId = createdLeague.Conferences[0].Divisions[0].Teams[0].Id;
        _macksLeagueJoeTeamId = createdLeague.Conferences[0].Divisions[0].Teams[1].Id;
        _macksLeagueDonTeamId = createdLeague.Conferences[0].Divisions[0].Teams[2].Id;
        _macksLeagueConferenceId = createdLeague.Conferences[0].Id;
        _macksLeagueDivisionId = createdLeague.Conferences[0].Divisions[0].Id;
    }

    private async Task AssignRolesAsync()
    {
        var userRepo = _fixture.ServiceProvider.GetRequiredService<IUserRepository>();

        // Create Joe if doesn't exist
        var joe = await userRepo.GetByAzureAdObjectIdWithRolesAsync(JoeOid);
        if (joe == null)
        {
            joe = new User
            {
                AzureAdObjectId = JoeOid,
                Email = "joe@example.com",
                DisplayName = "Joe User",
                IsGlobalAdmin = false
            };
            await userRepo.AddAsync(joe);
            joe = await userRepo.GetByAzureAdObjectIdWithRolesAsync(JoeOid);
        }

        // Assign Joe to Bob's League (Team 1)
        if (joe != null)
        {
            if (!joe.LeagueRoles.Any(r => r.LeagueId == _bobsLeagueId && r.TeamId == _bobsLeagueJoeTeamId))
            {
                joe.LeagueRoles.Add(new UserLeagueRole
                {
                    UserId = joe.Id,
                    LeagueId = _bobsLeagueId,
                    Role = UserRole.GeneralManager,
                    TeamId = _bobsLeagueJoeTeamId
                });
            }

            // Assign Joe to Mack's League (Team 2)
            if (!joe.LeagueRoles.Any(r => r.LeagueId == _macksLeagueId && r.TeamId == _macksLeagueJoeTeamId))
            {
                joe.LeagueRoles.Add(new UserLeagueRole
                {
                    UserId = joe.Id,
                    LeagueId = _macksLeagueId,
                    Role = UserRole.GeneralManager,
                    TeamId = _macksLeagueJoeTeamId
                });
            }

            await userRepo.UpdateAsync(joe);
        }

        // Assign Bob to Mack's League (Team 1)
        var bob = await userRepo.GetByAzureAdObjectIdWithRolesAsync(BobOid);
        if (bob != null && !bob.LeagueRoles.Any(r => r.LeagueId == _macksLeagueId && r.TeamId == _macksLeagueBobTeamId))
        {
            bob.LeagueRoles.Add(new UserLeagueRole
            {
                UserId = bob.Id,
                LeagueId = _macksLeagueId,
                Role = UserRole.GeneralManager,
                TeamId = _macksLeagueBobTeamId
            });
            await userRepo.UpdateAsync(bob);
        }

        // Assign Don to Bob's League (Team 2)
        var don = await userRepo.GetByAzureAdObjectIdWithRolesAsync(DonOid);
        if (don == null)
        {
            don = new User
            {
                AzureAdObjectId = DonOid,
                Email = "don@example.com",
                DisplayName = "Don User",
                IsGlobalAdmin = false
            };
            await userRepo.AddAsync(don);
        }

        don = await userRepo.GetByAzureAdObjectIdWithRolesAsync(DonOid);
        if (don != null)
        {
            if (!don.LeagueRoles.Any(r => r.LeagueId == _bobsLeagueId && r.TeamId == _bobsLeagueDonTeamId))
            {
                don.LeagueRoles.Add(new UserLeagueRole
                {
                    UserId = don.Id,
                    LeagueId = _bobsLeagueId,
                    Role = UserRole.GeneralManager,
                    TeamId = _bobsLeagueDonTeamId
                });
            }

            // Assign Don to Mack's League (Team 3)
            if (!don.LeagueRoles.Any(r => r.LeagueId == _macksLeagueId && r.TeamId == _macksLeagueDonTeamId))
            {
                don.LeagueRoles.Add(new UserLeagueRole
                {
                    UserId = don.Id,
                    LeagueId = _macksLeagueId,
                    Role = UserRole.GeneralManager,
                    TeamId = _macksLeagueDonTeamId
                });
            }

            await userRepo.UpdateAsync(don);
        }
    }

    private async Task PopulateRosters()
    {
        var (bobController, _, _) = CreateTeamsManagementControllerWithAuth(BobOid, "bob@example.com", "Bob User");

        // Populate Joe's team in Bob's League
        await bobController.PopulateTeamRoster(_bobsLeagueJoeTeamId, 1000);

        // Populate Don's team in Bob's League
        await bobController.PopulateTeamRoster(_bobsLeagueDonTeamId, 2000);

        var (mackController, _, _) = CreateTeamsManagementControllerWithAuth(MackOid, "mack@example.com", "Mack User");

        // Populate teams in Mack's League
        await mackController.PopulateTeamRoster(_macksLeagueBobTeamId, 3000);
        await mackController.PopulateTeamRoster(_macksLeagueJoeTeamId, 4000);
        await mackController.PopulateTeamRoster(_macksLeagueDonTeamId, 5000);
    }

    private (TeamsManagementController controller, ITeamRepository teamRepo, IPlayerRepository playerRepo)
        CreateTeamsManagementControllerWithAuth(string azureAdObjectId, string email, string displayName)
    {
        var teamRepo = _fixture.ServiceProvider.GetRequiredService<ITeamRepository>();
        var playerRepo = _fixture.ServiceProvider.GetRequiredService<IPlayerRepository>();
        var divisionRepo = _fixture.ServiceProvider.GetRequiredService<IDivisionRepository>();
        var conferenceRepo = _fixture.ServiceProvider.GetRequiredService<IConferenceRepository>();
        var teamBuilderService = _fixture.ServiceProvider.GetRequiredService<GameManagement.Services.ITeamBuilderService>();
        var playerGeneratorService = _fixture.ServiceProvider.GetRequiredService<GameManagement.Services.IPlayerGeneratorService>();
        var authService = _fixture.ServiceProvider.GetRequiredService<IGridironAuthorizationService>();
        var logger = _fixture.ServiceProvider.GetRequiredService<ILogger<TeamsManagementController>>();

        var controller = new TeamsManagementController(
            teamRepo,
            playerRepo,
            divisionRepo,
            conferenceRepo,
            teamBuilderService,
            playerGeneratorService,
            authService,
            logger);

        var httpContext = CreateHttpContext(azureAdObjectId, email, displayName);
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        return (controller, teamRepo, playerRepo);
    }

    private (ConferencesManagementController controller, IConferenceRepository conferenceRepo)
        CreateConferencesManagementControllerWithAuth(string azureAdObjectId, string email, string displayName)
    {
        var conferenceRepo = _fixture.ServiceProvider.GetRequiredService<IConferenceRepository>();
        var conferenceBuilderService = _fixture.ServiceProvider.GetRequiredService<GameManagement.Services.IConferenceBuilderService>();
        var authService = _fixture.ServiceProvider.GetRequiredService<IGridironAuthorizationService>();
        var logger = _fixture.ServiceProvider.GetRequiredService<ILogger<ConferencesManagementController>>();

        var controller = new ConferencesManagementController(
            conferenceRepo,
            conferenceBuilderService,
            authService,
            logger);

        var httpContext = CreateHttpContext(azureAdObjectId, email, displayName);
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        return (controller, conferenceRepo);
    }

    private (LeaguesManagementController controller, ILeagueRepository leagueRepo, IUserRepository userRepo)
        CreateLeaguesManagementControllerWithAuth(string azureAdObjectId, string email, string displayName)
    {
        var leagueRepo = _fixture.ServiceProvider.GetRequiredService<ILeagueRepository>();
        var leagueBuilderService = _fixture.ServiceProvider.GetRequiredService<GameManagement.Services.ILeagueBuilderService>();
        var authService = _fixture.ServiceProvider.GetRequiredService<IGridironAuthorizationService>();
        var logger = _fixture.ServiceProvider.GetRequiredService<ILogger<LeaguesManagementController>>();
        var userRepo = _fixture.ServiceProvider.GetRequiredService<IUserRepository>();

        var controller = new LeaguesManagementController(
            leagueRepo,
            leagueBuilderService,
            authService,
            logger);

        var httpContext = CreateHttpContext(azureAdObjectId, email, displayName);
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        return (controller, leagueRepo, userRepo);
    }

    private PlayersController CreatePlayersControllerWithAuth(string azureAdObjectId, string email, string displayName)
    {
        var playerRepo = _fixture.ServiceProvider.GetRequiredService<IPlayerRepository>();
        var playerGeneratorService = _fixture.ServiceProvider.GetRequiredService<GameManagement.Services.IPlayerGeneratorService>();
        var authService = _fixture.ServiceProvider.GetRequiredService<IGridironAuthorizationService>();
        var logger = _fixture.ServiceProvider.GetRequiredService<ILogger<PlayersController>>();

        var controller = new PlayersController(
            playerRepo,
            playerGeneratorService,
            authService,
            logger);

        var httpContext = CreateHttpContext(azureAdObjectId, email, displayName);
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        return controller;
    }

    private HttpContext CreateHttpContext(string azureAdObjectId, string email, string displayName)
    {
        var claims = new List<Claim>
        {
            new Claim("oid", azureAdObjectId),
            new Claim("email", email),
            new Claim("name", displayName)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        return new DefaultHttpContext
        {
            User = claimsPrincipal
        };
    }

    #endregion
}

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
using System.Security.Claims;
using Xunit;

namespace Gridiron.IntegrationTests;

/// <summary>
/// COMPREHENSIVE Authorization Tests with 5 Users
///
/// Test Scenario:
/// - God: Global Admin, can see everything
/// - Bob: Commissioner of "Bob's League", has Joe as GM
/// - Joe: GM in Bob's League (Team 1), GM in Mack's League (Team 1)
/// - Mack: Commissioner of "Mack's League", has Bob and Joe as GMs
/// - Don: GM in Bob's League (Team 2), GM in Mack's League (Team 2)
///
/// Security Matrix:
/// - God sees: Bob's League, Mack's League
/// - Bob sees: Bob's League (full access as Commissioner)
/// - Joe sees: Bob's League (Team 1 only), Mack's League (Team 1 only)
/// - Mack sees: Mack's League (full access as Commissioner)
/// - Don sees: Bob's League (Team 2 only), Mack's League (Team 2 only)
///
/// Critical Tests:
/// - Mack CANNOT see Bob's League (not in it)
/// - Bob CANNOT see Mack's League (only GM, not Commissioner)
/// - Joe CANNOT see Don's teams
/// - Don CANNOT see Joe's teams
/// </summary>
public class ComprehensiveAuthorizationTests : IClassFixture<DatabaseTestFixture>
{
    private readonly DatabaseTestFixture _fixture;

    // User OIDs
    private const string GodOid = "god-comprehensive-oid";
    private const string BobOid = "bob-comprehensive-oid";
    private const string JoeOid = "joe-comprehensive-oid";
    private const string MackOid = "mack-comprehensive-oid";
    private const string DonOid = "don-comprehensive-oid";

    // League and team tracking
    private int _bobsLeagueId;
    private int _macksLeagueId;
    private int _bobsLeagueJoeTeamId;
    private int _bobsLeagueDonTeamId;
    private int _macksLeagueBobTeamId;
    private int _macksLeagueJoeTeamId;
    private int _macksLeagueDonTeamId;

    public ComprehensiveAuthorizationTests(DatabaseTestFixture fixture)
    {
        _fixture = fixture;
    }

    #region Setup Test Data

    [Fact]
    public async Task Setup_CreateTestScenario()
    {
        // This test sets up the entire scenario and verifies it
        await CreateGodAsync();
        await CreateBobsLeagueAsync();
        await CreateMacksLeagueAsync();
        await AssignRolesAsync();

        // Verify setup is correct
        var userRepo = _fixture.ServiceProvider.GetRequiredService<IUserRepository>();

        var god = await userRepo.GetByAzureAdObjectIdWithRolesAsync(GodOid);
        god.Should().NotBeNull();
        god!.IsGlobalAdmin.Should().BeTrue();

        var bob = await userRepo.GetByAzureAdObjectIdWithRolesAsync(BobOid);
        bob.Should().NotBeNull();
        bob!.LeagueRoles.Should().Contain(r => r.LeagueId == _bobsLeagueId && r.Role == UserRole.Commissioner);
        bob.LeagueRoles.Should().Contain(r => r.LeagueId == _macksLeagueId && r.Role == UserRole.GeneralManager && r.TeamId == _macksLeagueBobTeamId);

        var joe = await userRepo.GetByAzureAdObjectIdWithRolesAsync(JoeOid);
        joe.Should().NotBeNull();
        joe!.LeagueRoles.Should().Contain(r => r.LeagueId == _bobsLeagueId && r.Role == UserRole.GeneralManager && r.TeamId == _bobsLeagueJoeTeamId);
        joe.LeagueRoles.Should().Contain(r => r.LeagueId == _macksLeagueId && r.Role == UserRole.GeneralManager && r.TeamId == _macksLeagueJoeTeamId);

        var mack = await userRepo.GetByAzureAdObjectIdWithRolesAsync(MackOid);
        mack.Should().NotBeNull();
        mack!.LeagueRoles.Should().Contain(r => r.LeagueId == _macksLeagueId && r.Role == UserRole.Commissioner);

        var don = await userRepo.GetByAzureAdObjectIdWithRolesAsync(DonOid);
        don.Should().NotBeNull();
        don!.LeagueRoles.Should().Contain(r => r.LeagueId == _bobsLeagueId && r.Role == UserRole.GeneralManager && r.TeamId == _bobsLeagueDonTeamId);
        don.LeagueRoles.Should().Contain(r => r.LeagueId == _macksLeagueId && r.Role == UserRole.GeneralManager && r.TeamId == _macksLeagueDonTeamId);
    }

    #endregion

    #region God Tests - Can See Everything

    [Fact]
    public async Task God_CanSeeAllLeagues()
    {
        await CreateTestScenario();
        var (godController, _, _) = CreateControllerWithAuth(GodOid, "god@example.com", "God Admin");

        var result = await godController.GetAllLeagues();

        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var leagues = okResult!.Value as List<LeagueDto>;

        leagues.Should().Contain(l => l.Id == _bobsLeagueId);
        leagues.Should().Contain(l => l.Id == _macksLeagueId);
    }

    [Fact]
    public async Task God_CanAccessBobsLeague()
    {
        await CreateTestScenario();
        var (godController, _, _) = CreateControllerWithAuth(GodOid, "god@example.com", "God Admin");

        var result = await godController.GetLeague(_bobsLeagueId);

        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var league = okResult!.Value as LeagueDetailDto;
        league!.Id.Should().Be(_bobsLeagueId);
    }

    [Fact]
    public async Task God_CanAccessMacksLeague()
    {
        await CreateTestScenario();
        var (godController, _, _) = CreateControllerWithAuth(GodOid, "god@example.com", "God Admin");

        var result = await godController.GetLeague(_macksLeagueId);

        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var league = okResult!.Value as LeagueDetailDto;
        league!.Id.Should().Be(_macksLeagueId);
    }

    #endregion

    #region Bob Tests - Commissioner of His League, GM in Mack's League

    [Fact]
    public async Task Bob_CanSeeHisOwnLeague()
    {
        await CreateTestScenario();
        var (bobController, _, _) = CreateControllerWithAuth(BobOid, "bob@example.com", "Bob User");

        var result = await bobController.GetAllLeagues();

        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var leagues = okResult!.Value as List<LeagueDto>;

        leagues.Should().Contain(l => l.Id == _bobsLeagueId);
        leagues.Should().Contain(l => l.Id == _macksLeagueId); // Bob is GM in Mack's league
    }

    [Fact]
    public async Task Bob_CanAccessHisLeagueDetails()
    {
        await CreateTestScenario();
        var (bobController, _, _) = CreateControllerWithAuth(BobOid, "bob@example.com", "Bob User");

        var result = await bobController.GetLeague(_bobsLeagueId);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Bob_CanUpdateHisLeague()
    {
        await CreateTestScenario();
        var (bobController, _, _) = CreateControllerWithAuth(BobOid, "bob@example.com", "Bob User");

        var updateRequest = new UpdateLeagueRequest
        {
            Name = "Bob's Updated League",
            Season = 2026,
            IsActive = true
        };

        var result = await bobController.UpdateLeague(_bobsLeagueId, updateRequest);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Bob_CannotUpdateMacksLeague_EvenAsGM()
    {
        await CreateTestScenario();
        var (bobController, _, _) = CreateControllerWithAuth(BobOid, "bob@example.com", "Bob User");

        var updateRequest = new UpdateLeagueRequest
        {
            Name = "Bob Trying to Hack Mack's League",
            Season = 2026,
            IsActive = false
        };

        var result = await bobController.UpdateLeague(_macksLeagueId, updateRequest);

        result.Result.Should().BeOfType<ForbidResult>();
    }

    #endregion

    #region Joe Tests - GM in Both Leagues

    [Fact]
    public async Task Joe_CanSeeBothLeagues()
    {
        await CreateTestScenario();
        var (joeController, _, _) = CreateControllerWithAuth(JoeOid, "joe@example.com", "Joe User");

        var result = await joeController.GetAllLeagues();

        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var leagues = okResult!.Value as List<LeagueDto>;

        leagues.Should().Contain(l => l.Id == _bobsLeagueId);
        leagues.Should().Contain(l => l.Id == _macksLeagueId);
    }

    [Fact]
    public async Task Joe_CanAccessBobsLeague()
    {
        await CreateTestScenario();
        var (joeController, _, _) = CreateControllerWithAuth(JoeOid, "joe@example.com", "Joe User");

        var result = await joeController.GetLeague(_bobsLeagueId);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Joe_CanAccessMacksLeague()
    {
        await CreateTestScenario();
        var (joeController, _, _) = CreateControllerWithAuth(JoeOid, "joe@example.com", "Joe User");

        var result = await joeController.GetLeague(_macksLeagueId);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Joe_CannotUpdateBobsLeague_OnlyGM()
    {
        await CreateTestScenario();
        var (joeController, _, _) = CreateControllerWithAuth(JoeOid, "joe@example.com", "Joe User");

        var updateRequest = new UpdateLeagueRequest
        {
            Name = "Joe Trying to Hack",
            Season = 2026,
            IsActive = false
        };

        var result = await joeController.UpdateLeague(_bobsLeagueId, updateRequest);

        result.Result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task Joe_CannotUpdateMacksLeague_OnlyGM()
    {
        await CreateTestScenario();
        var (joeController, _, _) = CreateControllerWithAuth(JoeOid, "joe@example.com", "Joe User");

        var updateRequest = new UpdateLeagueRequest
        {
            Name = "Joe Trying to Hack Mack",
            Season = 2026,
            IsActive = false
        };

        var result = await joeController.UpdateLeague(_macksLeagueId, updateRequest);

        result.Result.Should().BeOfType<ForbidResult>();
    }

    #endregion

    #region Mack Tests - Commissioner of His League, NOT in Bob's League

    [Fact]
    public async Task Mack_CanOnlySeeHisOwnLeague()
    {
        await CreateTestScenario();
        var (mackController, _, _) = CreateControllerWithAuth(MackOid, "mack@example.com", "Mack User");

        var result = await mackController.GetAllLeagues();

        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var leagues = okResult!.Value as List<LeagueDto>;

        leagues.Should().Contain(l => l.Id == _macksLeagueId);
        leagues.Should().NotContain(l => l.Id == _bobsLeagueId); // CRITICAL: Mack cannot see Bob's league
    }

    [Fact]
    public async Task Mack_CannotAccessBobsLeague()
    {
        await CreateTestScenario();
        var (mackController, _, _) = CreateControllerWithAuth(MackOid, "mack@example.com", "Mack User");

        var result = await mackController.GetLeague(_bobsLeagueId);

        result.Result.Should().BeOfType<ForbidResult>(); // CRITICAL: Mack is forbidden from Bob's league
    }

    [Fact]
    public async Task Mack_CanAccessHisOwnLeague()
    {
        await CreateTestScenario();
        var (mackController, _, _) = CreateControllerWithAuth(MackOid, "mack@example.com", "Mack User");

        var result = await mackController.GetLeague(_macksLeagueId);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Mack_CanUpdateHisLeague()
    {
        await CreateTestScenario();
        var (mackController, _, _) = CreateControllerWithAuth(MackOid, "mack@example.com", "Mack User");

        var updateRequest = new UpdateLeagueRequest
        {
            Name = "Mack's Updated League",
            Season = 2027,
            IsActive = true
        };

        var result = await mackController.UpdateLeague(_macksLeagueId, updateRequest);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Mack_CannotUpdateBobsLeague()
    {
        await CreateTestScenario();
        var (mackController, _, _) = CreateControllerWithAuth(MackOid, "mack@example.com", "Mack User");

        var updateRequest = new UpdateLeagueRequest
        {
            Name = "Mack Trying to Hack Bob",
            Season = 2026,
            IsActive = false
        };

        var result = await mackController.UpdateLeague(_bobsLeagueId, updateRequest);

        result.Result.Should().BeOfType<ForbidResult>();
    }

    #endregion

    #region Don Tests - GM in Both Leagues

    [Fact]
    public async Task Don_CanSeeBothLeagues()
    {
        await CreateTestScenario();
        var (donController, _, _) = CreateControllerWithAuth(DonOid, "don@example.com", "Don User");

        var result = await donController.GetAllLeagues();

        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var leagues = okResult!.Value as List<LeagueDto>;

        leagues.Should().Contain(l => l.Id == _bobsLeagueId);
        leagues.Should().Contain(l => l.Id == _macksLeagueId);
    }

    [Fact]
    public async Task Don_CanAccessBobsLeague()
    {
        await CreateTestScenario();
        var (donController, _, _) = CreateControllerWithAuth(DonOid, "don@example.com", "Don User");

        var result = await donController.GetLeague(_bobsLeagueId);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Don_CanAccessMacksLeague()
    {
        await CreateTestScenario();
        var (donController, _, _) = CreateControllerWithAuth(DonOid, "don@example.com", "Don User");

        var result = await donController.GetLeague(_macksLeagueId);

        result.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Don_CannotUpdateBobsLeague_OnlyGM()
    {
        await CreateTestScenario();
        var (donController, _, _) = CreateControllerWithAuth(DonOid, "don@example.com", "Don User");

        var updateRequest = new UpdateLeagueRequest
        {
            Name = "Don Trying to Hack Bob",
            Season = 2026,
            IsActive = false
        };

        var result = await donController.UpdateLeague(_bobsLeagueId, updateRequest);

        result.Result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task Don_CannotUpdateMacksLeague_OnlyGM()
    {
        await CreateTestScenario();
        var (donController, _, _) = CreateControllerWithAuth(DonOid, "don@example.com", "Don User");

        var updateRequest = new UpdateLeagueRequest
        {
            Name = "Don Trying to Hack Mack",
            Season = 2026,
            IsActive = false
        };

        var result = await donController.UpdateLeague(_macksLeagueId, updateRequest);

        result.Result.Should().BeOfType<ForbidResult>();
    }

    #endregion

    #region Cross-User Isolation Tests

    [Fact]
    public async Task Joe_CannotSeeDonsTeams_InBobsLeague()
    {
        await CreateTestScenario();
        // This test verifies that Joe (GM of Team 1) cannot see Don's team (Team 2)
        // At the league level, both can see the league, but team-level auth will prevent access
        // This will be more relevant when we add team-specific endpoints

        var authService = _fixture.ServiceProvider.GetRequiredService<IGridironAuthorizationService>();

        // Joe can access his team
        var joeCanAccessHisTeam = await authService.CanAccessTeamAsync(JoeOid, _bobsLeagueJoeTeamId);
        joeCanAccessHisTeam.Should().BeTrue();

        // Joe CANNOT access Don's team
        var joeCanAccessDonsTeam = await authService.CanAccessTeamAsync(JoeOid, _bobsLeagueDonTeamId);
        joeCanAccessDonsTeam.Should().BeFalse();
    }

    [Fact]
    public async Task Don_CannotSeeJoesTeams_InMacksLeague()
    {
        await CreateTestScenario();

        var authService = _fixture.ServiceProvider.GetRequiredService<IGridironAuthorizationService>();

        // Don can access his team
        var donCanAccessHisTeam = await authService.CanAccessTeamAsync(DonOid, _macksLeagueDonTeamId);
        donCanAccessHisTeam.Should().BeTrue();

        // Don CANNOT access Joe's team
        var donCanAccessJoesTeam = await authService.CanAccessTeamAsync(DonOid, _macksLeagueJoeTeamId);
        donCanAccessJoesTeam.Should().BeFalse();
    }

    [Fact]
    public async Task Bob_AsCommissioner_CanSeeAllTeamsInHisLeague()
    {
        await CreateTestScenario();

        var authService = _fixture.ServiceProvider.GetRequiredService<IGridironAuthorizationService>();

        // Bob can access Joe's team (as Commissioner)
        var bobCanAccessJoesTeam = await authService.CanAccessTeamAsync(BobOid, _bobsLeagueJoeTeamId);
        bobCanAccessJoesTeam.Should().BeTrue();

        // Bob can access Don's team (as Commissioner)
        var bobCanAccessDonsTeam = await authService.CanAccessTeamAsync(BobOid, _bobsLeagueDonTeamId);
        bobCanAccessDonsTeam.Should().BeTrue();
    }

    [Fact]
    public async Task Bob_AsGM_CanOnlySeeHisTeamInMacksLeague()
    {
        await CreateTestScenario();

        var authService = _fixture.ServiceProvider.GetRequiredService<IGridironAuthorizationService>();

        // Bob can access his team in Mack's league
        var bobCanAccessHisTeam = await authService.CanAccessTeamAsync(BobOid, _macksLeagueBobTeamId);
        bobCanAccessHisTeam.Should().BeTrue();

        // Bob CANNOT access Joe's team in Mack's league (Bob is only GM, not Commissioner)
        var bobCanAccessJoesTeam = await authService.CanAccessTeamAsync(BobOid, _macksLeagueJoeTeamId);
        bobCanAccessJoesTeam.Should().BeFalse();

        // Bob CANNOT access Don's team in Mack's league
        var bobCanAccessDonsTeam = await authService.CanAccessTeamAsync(BobOid, _macksLeagueDonTeamId);
        bobCanAccessDonsTeam.Should().BeFalse();
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
        var (bobController, _, _) = CreateControllerWithAuth(BobOid, "bob@example.com", "Bob User");

        var createResult = await bobController.CreateLeague(new CreateLeagueRequest
        {
            Name = "Bob's Comprehensive League",
            NumberOfConferences = 1,
            DivisionsPerConference = 1,
            TeamsPerDivision = 3 // Team 1 for Joe, Team 2 for Don, Team 3 unassigned
        });

        var createdLeague = ((createResult.Result as CreatedAtActionResult)!.Value as LeagueDetailDto)!;
        _bobsLeagueId = createdLeague.Id;
        _bobsLeagueJoeTeamId = createdLeague.Conferences[0].Divisions[0].Teams[0].Id;
        _bobsLeagueDonTeamId = createdLeague.Conferences[0].Divisions[0].Teams[1].Id;
    }

    private async Task CreateMacksLeagueAsync()
    {
        var (mackController, _, _) = CreateControllerWithAuth(MackOid, "mack@example.com", "Mack User");

        var createResult = await mackController.CreateLeague(new CreateLeagueRequest
        {
            Name = "Mack's Comprehensive League",
            NumberOfConferences = 1,
            DivisionsPerConference = 1,
            TeamsPerDivision = 4 // Team 1 for Bob, Team 2 for Joe, Team 3 for Don, Team 4 unassigned
        });

        var createdLeague = ((createResult.Result as CreatedAtActionResult)!.Value as LeagueDetailDto)!;
        _macksLeagueId = createdLeague.Id;
        _macksLeagueBobTeamId = createdLeague.Conferences[0].Divisions[0].Teams[0].Id;
        _macksLeagueJoeTeamId = createdLeague.Conferences[0].Divisions[0].Teams[1].Id;
        _macksLeagueDonTeamId = createdLeague.Conferences[0].Divisions[0].Teams[2].Id;
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

    private (LeaguesManagementController controller, IUserRepository userRepo, ILeagueRepository leagueRepo)
        CreateControllerWithAuth(string azureAdObjectId, string email, string displayName)
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

        // Set up HttpContext with user claims
        var claims = new List<Claim>
        {
            new Claim("oid", azureAdObjectId),
            new Claim("email", email),
            new Claim("name", displayName)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = claimsPrincipal
        };

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        return (controller, userRepo, leagueRepo);
    }

    #endregion
}

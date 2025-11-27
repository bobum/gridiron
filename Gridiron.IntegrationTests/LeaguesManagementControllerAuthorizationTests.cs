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
/// Integration tests for LeaguesManagementController Authorization
/// CRITICAL: These tests verify that users can only access their own resources
/// Tests the FULL STACK: Controller → Service → Repository → Database
/// </summary>
public class LeaguesManagementControllerAuthorizationTests : IClassFixture<DatabaseTestFixture>
{
    private readonly DatabaseTestFixture _fixture;

    public LeaguesManagementControllerAuthorizationTests(DatabaseTestFixture fixture)
    {
        _fixture = fixture;
    }

    #region CreateLeague Authorization Tests

    [Fact]
    public async Task CreateLeague_AutoAssignsCreatorAsCommissioner()
    {
        // Arrange
        var (controller, userRepo, leagueRepo) = CreateControllerWithAuth("joe-oid", "joe@example.com", "Joe User");

        var request = new CreateLeagueRequest
        {
            Name = "Joe's League",
            NumberOfConferences = 2,
            DivisionsPerConference = 2,
            TeamsPerDivision = 2
        };

        // Act
        var result = await controller.CreateLeague(request);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result.Result as CreatedAtActionResult;
        var leagueDto = createdResult!.Value as LeagueDetailDto;

        // Verify league was created
        leagueDto.Should().NotBeNull();
        leagueDto!.Name.Should().Be("Joe's League");

        // Verify Joe is the commissioner
        var user = await userRepo.GetByAzureAdObjectIdWithRolesAsync("joe-oid");
        user.Should().NotBeNull();
        user!.LeagueRoles.Should().ContainSingle(r =>
            r.LeagueId == leagueDto.Id &&
            r.Role == UserRole.Commissioner &&
            r.TeamId == null);
    }

    #endregion

    #region GetLeague Authorization Tests

    [Fact]
    public async Task GetLeague_UserCanAccessTheirOwnLeague()
    {
        // Arrange - Create league as Joe
        var (joeController, joeUserRepo, leagueRepo) = CreateControllerWithAuth("joe-oid", "joe@example.com", "Joe User");
        var createResult = await joeController.CreateLeague(new CreateLeagueRequest
        {
            Name = "Joe's League",
            NumberOfConferences = 1,
            DivisionsPerConference = 1,
            TeamsPerDivision = 2
        });
        var createdLeague = ((createResult.Result as CreatedAtActionResult)!.Value as LeagueDetailDto)!;

        // Act - Joe tries to access his own league
        var getResult = await joeController.GetLeague(createdLeague.Id);

        // Assert - Joe can access it
        getResult.Result.Should().BeOfType<OkObjectResult>();
        var okResult = getResult.Result as OkObjectResult;
        var leagueDto = okResult!.Value as LeagueDetailDto;
        leagueDto!.Id.Should().Be(createdLeague.Id);
        leagueDto.Name.Should().Be("Joe's League");
    }

    [Fact]
    public async Task GetLeague_UserCannotAccessOtherUsersLeague()
    {
        // Arrange - Create league as Joe
        var (joeController, joeUserRepo, leagueRepo) = CreateControllerWithAuth("joe-oid", "joe@example.com", "Joe User");
        var createResult = await joeController.CreateLeague(new CreateLeagueRequest
        {
            Name = "Joe's League",
            NumberOfConferences = 1,
            DivisionsPerConference = 1,
            TeamsPerDivision = 2
        });
        var createdLeague = ((createResult.Result as CreatedAtActionResult)!.Value as LeagueDetailDto)!;

        // Act - Bob tries to access Joe's league
        var (bobController, _, _) = CreateControllerWithAuth("bob-oid", "bob@example.com", "Bob User");
        var getResult = await bobController.GetLeague(createdLeague.Id);

        // Assert - Bob is forbidden
        getResult.Result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task GetLeague_GlobalAdminCanAccessAnyLeague()
    {
        // Arrange - Create league as Joe
        var (joeController, joeUserRepo, leagueRepo) = CreateControllerWithAuth("joe-oid", "joe@example.com", "Joe User");
        var createResult = await joeController.CreateLeague(new CreateLeagueRequest
        {
            Name = "Joe's League",
            NumberOfConferences = 1,
            DivisionsPerConference = 1,
            TeamsPerDivision = 2
        });
        var createdLeague = ((createResult.Result as CreatedAtActionResult)!.Value as LeagueDetailDto)!;

        // Arrange - Make God a global admin
        var userRepo = _fixture.ServiceProvider.GetRequiredService<IUserRepository>();
        var god = new User
        {
            AzureAdObjectId = "god-oid",
            Email = "god@example.com",
            DisplayName = "God Admin",
            IsGlobalAdmin = true
        };
        await userRepo.AddAsync(god);

        // Act - God tries to access Joe's league
        var (godController, _, _) = CreateControllerWithAuth("god-oid", "god@example.com", "God Admin");
        var getResult = await godController.GetLeague(createdLeague.Id);

        // Assert - God can access it
        getResult.Result.Should().BeOfType<OkObjectResult>();
        var okResult = getResult.Result as OkObjectResult;
        var leagueDto = okResult!.Value as LeagueDetailDto;
        leagueDto!.Id.Should().Be(createdLeague.Id);
    }

    [Fact]
    public async Task GetLeague_GMCanAccessLeagueWhereTheyHaveTeam()
    {
        // Arrange - Create league as Commissioner
        var (commishController, commishUserRepo, leagueRepo) = CreateControllerWithAuth("commish-oid", "commish@example.com", "Commissioner User");
        var createResult = await commishController.CreateLeague(new CreateLeagueRequest
        {
            Name = "Test League",
            NumberOfConferences = 1,
            DivisionsPerConference = 1,
            TeamsPerDivision = 2
        });
        var createdLeague = ((createResult.Result as CreatedAtActionResult)!.Value as LeagueDetailDto)!;
        var teamId = createdLeague.Conferences[0].Divisions[0].Teams[0].Id;

        // Arrange - Assign GM to a team in the league
        var userRepo = _fixture.ServiceProvider.GetRequiredService<IUserRepository>();
        var gm = new User
        {
            AzureAdObjectId = "gm-oid",
            Email = "gm@example.com",
            DisplayName = "GM User",
            IsGlobalAdmin = false
        };
        await userRepo.AddAsync(gm);

        gm.LeagueRoles.Add(new UserLeagueRole
        {
            UserId = gm.Id,
            LeagueId = createdLeague.Id,
            Role = UserRole.GeneralManager,
            TeamId = teamId
        });
        await userRepo.UpdateAsync(gm);

        // Act - GM tries to access the league where they have a team
        var (gmController, _, _) = CreateControllerWithAuth("gm-oid", "gm@example.com", "GM User");
        var getResult = await gmController.GetLeague(createdLeague.Id);

        // Assert - GM can access it
        getResult.Result.Should().BeOfType<OkObjectResult>();
        var okResult = getResult.Result as OkObjectResult;
        var leagueDto = okResult!.Value as LeagueDetailDto;
        leagueDto!.Id.Should().Be(createdLeague.Id);
    }

    #endregion

    #region GetAllLeagues Authorization Tests

    [Fact]
    public async Task GetAllLeagues_UserOnlySeesTheirOwnLeagues()
    {
        // Arrange - Joe creates a league
        var (joeController, joeUserRepo, leagueRepo) = CreateControllerWithAuth("joe-oid", "joe@example.com", "Joe User");
        await joeController.CreateLeague(new CreateLeagueRequest
        {
            Name = "Joe's NFL",
            NumberOfConferences = 1,
            DivisionsPerConference = 1,
            TeamsPerDivision = 2
        });

        // Arrange - Bob creates a different league
        var (bobController, bobUserRepo, _) = CreateControllerWithAuth("bob-oid", "bob@example.com", "Bob User");
        await bobController.CreateLeague(new CreateLeagueRequest
        {
            Name = "Bob's CFL",
            NumberOfConferences = 1,
            DivisionsPerConference = 1,
            TeamsPerDivision = 2
        });

        // Act - Joe gets all leagues
        var joeResult = await joeController.GetAllLeagues();

        // Assert - Joe only sees leagues he created (including this one)
        joeResult.Result.Should().BeOfType<OkObjectResult>();
        var joeOkResult = joeResult.Result as OkObjectResult;
        var joeLeagues = joeOkResult!.Value as List<LeagueDto>;
        joeLeagues.Should().Contain(l => l.Name == "Joe's NFL");
        joeLeagues.Should().NotContain(l => l.Name == "Bob's CFL");

        // Act - Bob gets all leagues
        var bobResult = await bobController.GetAllLeagues();

        // Assert - Bob only sees leagues he created (including this one)
        bobResult.Result.Should().BeOfType<OkObjectResult>();
        var bobOkResult = bobResult.Result as OkObjectResult;
        var bobLeagues = bobOkResult!.Value as List<LeagueDto>;
        bobLeagues.Should().Contain(l => l.Name == "Bob's CFL");
        bobLeagues.Should().NotContain(l => l.Name == "Joe's NFL");
    }

    [Fact]
    public async Task GetAllLeagues_GlobalAdminSeesAllLeagues()
    {
        // Arrange - Joe creates a league
        var (joeController, _, _) = CreateControllerWithAuth("joe-oid-2", "joe2@example.com", "Joe User 2");
        await joeController.CreateLeague(new CreateLeagueRequest
        {
            Name = "Joe's League 2",
            NumberOfConferences = 1,
            DivisionsPerConference = 1,
            TeamsPerDivision = 2
        });

        // Arrange - Bob creates a league
        var (bobController, _, _) = CreateControllerWithAuth("bob-oid-2", "bob2@example.com", "Bob User 2");
        await bobController.CreateLeague(new CreateLeagueRequest
        {
            Name = "Bob's League 2",
            NumberOfConferences = 1,
            DivisionsPerConference = 1,
            TeamsPerDivision = 2
        });

        // Arrange - Make God a global admin
        var userRepo = _fixture.ServiceProvider.GetRequiredService<IUserRepository>();
        var existingGod = await userRepo.GetByAzureAdObjectIdAsync("god-oid-2");
        if (existingGod == null)
        {
            var god = new User
            {
                AzureAdObjectId = "god-oid-2",
                Email = "god2@example.com",
                DisplayName = "God Admin 2",
                IsGlobalAdmin = true
            };
            await userRepo.AddAsync(god);
        }
        else
        {
            existingGod.IsGlobalAdmin = true;
            await userRepo.UpdateAsync(existingGod);
        }

        // Act - God gets all leagues
        var (godController, _, _) = CreateControllerWithAuth("god-oid-2", "god2@example.com", "God Admin 2");
        var godResult = await godController.GetAllLeagues();

        // Assert - God sees both leagues (plus any from other tests)
        godResult.Result.Should().BeOfType<OkObjectResult>();
        var godOkResult = godResult.Result as OkObjectResult;
        var godLeagues = godOkResult!.Value as List<LeagueDto>;
        godLeagues.Should().Contain(l => l.Name == "Joe's League 2");
        godLeagues.Should().Contain(l => l.Name == "Bob's League 2");
    }

    #endregion

    #region UpdateLeague Authorization Tests

    [Fact]
    public async Task UpdateLeague_CommissionerCanUpdateTheirLeague()
    {
        // Arrange - Joe creates a league
        var (joeController, _, leagueRepo) = CreateControllerWithAuth("joe-update-oid", "joe-update@example.com", "Joe Update");
        var createResult = await joeController.CreateLeague(new CreateLeagueRequest
        {
            Name = "Original Name",
            NumberOfConferences = 1,
            DivisionsPerConference = 1,
            TeamsPerDivision = 2
        });
        var createdLeague = ((createResult.Result as CreatedAtActionResult)!.Value as LeagueDetailDto)!;

        // Act - Joe updates his league
        var updateRequest = new UpdateLeagueRequest
        {
            Name = "Updated Name",
            Season = 2025,
            IsActive = true
        };
        var updateResult = await joeController.UpdateLeague(createdLeague.Id, updateRequest);

        // Assert - Update succeeds
        updateResult.Result.Should().BeOfType<OkObjectResult>();
        var okResult = updateResult.Result as OkObjectResult;
        var updatedDto = okResult!.Value as LeagueDto;
        updatedDto!.Name.Should().Be("Updated Name");
        updatedDto.Season.Should().Be(2025);
    }

    [Fact]
    public async Task UpdateLeague_NonCommissionerCannotUpdateOthersLeague()
    {
        // Arrange - Joe creates a league
        var (joeController, _, _) = CreateControllerWithAuth("joe-update2-oid", "joe-update2@example.com", "Joe Update 2");
        var createResult = await joeController.CreateLeague(new CreateLeagueRequest
        {
            Name = "Joe's Protected League",
            NumberOfConferences = 1,
            DivisionsPerConference = 1,
            TeamsPerDivision = 2
        });
        var createdLeague = ((createResult.Result as CreatedAtActionResult)!.Value as LeagueDetailDto)!;

        // Act - Bob tries to update Joe's league
        var (bobController, _, _) = CreateControllerWithAuth("bob-update-oid", "bob-update@example.com", "Bob Update");
        var updateRequest = new UpdateLeagueRequest
        {
            Name = "Hacked Name",
            Season = 2025,
            IsActive = false
        };
        var updateResult = await bobController.UpdateLeague(createdLeague.Id, updateRequest);

        // Assert - Bob is forbidden
        updateResult.Result.Should().BeOfType<ForbidResult>();

        // Verify league wasn't changed
        var league = await _fixture.ServiceProvider.GetRequiredService<ILeagueRepository>().GetByIdAsync(createdLeague.Id);
        league!.Name.Should().Be("Joe's Protected League");
    }

    [Fact]
    public async Task UpdateLeague_GMCannotUpdateLeague()
    {
        // Arrange - Create league as Commissioner
        var (commishController, _, leagueRepo) = CreateControllerWithAuth("commish-update-oid", "commish-update@example.com", "Commissioner Update");
        var createResult = await commishController.CreateLeague(new CreateLeagueRequest
        {
            Name = "Commissioner's League",
            NumberOfConferences = 1,
            DivisionsPerConference = 1,
            TeamsPerDivision = 2
        });
        var createdLeague = ((createResult.Result as CreatedAtActionResult)!.Value as LeagueDetailDto)!;
        var teamId = createdLeague.Conferences[0].Divisions[0].Teams[0].Id;

        // Arrange - Assign GM to a team
        var userRepo = _fixture.ServiceProvider.GetRequiredService<IUserRepository>();
        var gm = new User
        {
            AzureAdObjectId = "gm-update-oid",
            Email = "gm-update@example.com",
            DisplayName = "GM Update",
            IsGlobalAdmin = false
        };
        await userRepo.AddAsync(gm);

        gm.LeagueRoles.Add(new UserLeagueRole
        {
            UserId = gm.Id,
            LeagueId = createdLeague.Id,
            Role = UserRole.GeneralManager,
            TeamId = teamId
        });
        await userRepo.UpdateAsync(gm);

        // Act - GM tries to update league
        var (gmController, _, _) = CreateControllerWithAuth("gm-update-oid", "gm-update@example.com", "GM Update");
        var updateRequest = new UpdateLeagueRequest
        {
            Name = "GM Hacked League",
            Season = 2025,
            IsActive = false
        };
        var updateResult = await gmController.UpdateLeague(createdLeague.Id, updateRequest);

        // Assert - GM is forbidden
        updateResult.Result.Should().BeOfType<ForbidResult>();
    }

    #endregion

    #region DeleteLeague Authorization Tests

    [Fact]
    public async Task DeleteLeague_CommissionerCanDeleteTheirLeague()
    {
        // Arrange - Joe creates a league
        var (joeController, _, leagueRepo) = CreateControllerWithAuth("joe-delete-oid", "joe-delete@example.com", "Joe Delete");
        var createResult = await joeController.CreateLeague(new CreateLeagueRequest
        {
            Name = "League To Delete",
            NumberOfConferences = 1,
            DivisionsPerConference = 1,
            TeamsPerDivision = 2
        });
        var createdLeague = ((createResult.Result as CreatedAtActionResult)!.Value as LeagueDetailDto)!;

        // Act - Joe deletes his league
        var deleteResult = await joeController.DeleteLeague(createdLeague.Id, "Joe", "Testing deletion");

        // Assert - Delete succeeds
        deleteResult.Result.Should().BeOfType<OkObjectResult>();
        var okResult = deleteResult.Result as OkObjectResult;
        var cascadeResult = okResult!.Value as CascadeDeleteResult;
        cascadeResult!.Success.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteLeague_NonCommissionerCannotDeleteOthersLeague()
    {
        // Arrange - Joe creates a league
        var (joeController, _, _) = CreateControllerWithAuth("joe-delete2-oid", "joe-delete2@example.com", "Joe Delete 2");
        var createResult = await joeController.CreateLeague(new CreateLeagueRequest
        {
            Name = "Joe's Protected League 2",
            NumberOfConferences = 1,
            DivisionsPerConference = 1,
            TeamsPerDivision = 2
        });
        var createdLeague = ((createResult.Result as CreatedAtActionResult)!.Value as LeagueDetailDto)!;

        // Act - Bob tries to delete Joe's league
        var (bobController, _, _) = CreateControllerWithAuth("bob-delete-oid", "bob-delete@example.com", "Bob Delete");
        var deleteResult = await bobController.DeleteLeague(createdLeague.Id, "Bob", "Trying to hack");

        // Assert - Bob is forbidden
        deleteResult.Result.Should().BeOfType<ForbidResult>();
    }

    #endregion

    #region Helper Methods

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

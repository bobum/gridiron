using DataAccessLayer.Repositories;
using DomainObjects;
using Gridiron.WebApi.DTOs;
using Gridiron.WebApi.Extensions;
using Gridiron.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gridiron.WebApi.Controllers;

/// <summary>
/// Controller for user management
/// DOES NOT access the database directly - uses repositories from DataAccessLayer
/// REQUIRES AUTHENTICATION: All endpoints require valid Azure AD JWT token
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly ILeagueRepository _leagueRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly IDivisionRepository _divisionRepository;
    private readonly IConferenceRepository _conferenceRepository;
    private readonly IGridironAuthorizationService _authorizationService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        IUserRepository userRepository,
        ILeagueRepository leagueRepository,
        ITeamRepository teamRepository,
        IDivisionRepository divisionRepository,
        IConferenceRepository conferenceRepository,
        IGridironAuthorizationService authorizationService,
        ILogger<UsersController> logger)
    {
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _leagueRepository = leagueRepository ?? throw new ArgumentNullException(nameof(leagueRepository));
        _teamRepository = teamRepository ?? throw new ArgumentNullException(nameof(teamRepository));
        _divisionRepository = divisionRepository ?? throw new ArgumentNullException(nameof(divisionRepository));
        _conferenceRepository = conferenceRepository ?? throw new ArgumentNullException(nameof(conferenceRepository));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets the current user's information
    /// </summary>
    /// <returns>Current user details with their league roles</returns>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        // Get current user from JWT claims
        var azureAdObjectId = HttpContext.GetAzureAdObjectId();
        if (string.IsNullOrEmpty(azureAdObjectId))
        {
            return Unauthorized(new { error = "User identity not found in token" });
        }

        var user = await _userRepository.GetByAzureAdObjectIdWithRolesAsync(azureAdObjectId);

        // In E2E test mode, create a test user if it doesn't exist
        if (user == null && azureAdObjectId == "e2e-test-user-object-id")
        {
            user = await _authorizationService.GetOrCreateUserFromClaimsAsync(
                azureAdObjectId,
                "e2e-test@gridiron.test",
                "E2E Test User"
            );
            // Make the test user a global admin for full access
            user.IsGlobalAdmin = true;
            await _userRepository.UpdateAsync(user);
            // Reload with roles
            user = await _userRepository.GetByAzureAdObjectIdWithRolesAsync(azureAdObjectId);
        }

        if (user == null)
        {
            return NotFound(new { error = "User not found" });
        }

        var userDto = MapToUserDto(user);
        return Ok(userDto);
    }

    /// <summary>
    /// Gets all users in a specific league (requires league access)
    /// </summary>
    /// <param name="leagueId">League ID</param>
    /// <returns>List of users with roles in the league</returns>
    [HttpGet("league/{leagueId}")]
    [ProducesResponseType(typeof(IEnumerable<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsersByLeague(int leagueId)
    {
        // Get current user from JWT claims
        var azureAdObjectId = HttpContext.GetAzureAdObjectId();
        if (string.IsNullOrEmpty(azureAdObjectId))
        {
            return Unauthorized(new { error = "User identity not found in token" });
        }

        // Check if league exists
        var league = await _leagueRepository.GetByIdAsync(leagueId);
        if (league == null)
        {
            return NotFound(new { error = $"League with ID {leagueId} not found" });
        }

        // Check authorization - must be able to access the league
        var hasAccess = await _authorizationService.CanAccessLeagueAsync(azureAdObjectId, leagueId);
        if (!hasAccess)
        {
            return Forbid();
        }

        // Get all users and filter to those with roles in this league
        var allUsers = await _userRepository.GetAllAsync();
        var usersInLeague = allUsers
            .Where(u => u.LeagueRoles.Any(lr => lr.LeagueId == leagueId && !lr.IsDeleted))
            .ToList();

        var userDtos = usersInLeague.Select(u => MapToUserDto(u, leagueId)).ToList();
        return Ok(userDtos);
    }

    /// <summary>
    /// Assigns a role to a user in a league (Commissioner or GM)
    /// Only global admins or league commissioners can assign roles
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="request">Role assignment details</param>
    /// <returns>Updated user information</returns>
    [HttpPost("{userId}/league-roles")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> AssignLeagueRole(int userId, [FromBody] AssignLeagueRoleRequest request)
    {
        // Get current user from JWT claims
        var azureAdObjectId = HttpContext.GetAzureAdObjectId();
        if (string.IsNullOrEmpty(azureAdObjectId))
        {
            return Unauthorized(new { error = "User identity not found in token" });
        }

        // Validate the role
        if (request.Role != "Commissioner" && request.Role != "GeneralManager")
        {
            return BadRequest(new { error = "Role must be 'Commissioner' or 'GeneralManager'" });
        }

        // If GM role, TeamId is required
        if (request.Role == "GeneralManager" && !request.TeamId.HasValue)
        {
            return BadRequest(new { error = "TeamId is required for GeneralManager role" });
        }

        // Check if user exists
        var user = await _userRepository.GetByIdWithRolesAsync(userId);
        if (user == null)
        {
            return NotFound(new { error = $"User with ID {userId} not found" });
        }

        // Check if league exists
        var league = await _leagueRepository.GetByIdAsync(request.LeagueId);
        if (league == null)
        {
            return NotFound(new { error = $"League with ID {request.LeagueId} not found" });
        }

        // If GM role, check if team exists and belongs to the league
        if (request.TeamId.HasValue)
        {
            var team = await _teamRepository.GetByIdAsync(request.TeamId.Value);
            if (team == null)
            {
                return NotFound(new { error = $"Team with ID {request.TeamId.Value} not found" });
            }

            // Verify team belongs to the league
            var teamLeagueId = await GetTeamLeagueIdAsync(request.TeamId.Value);
            if (teamLeagueId != request.LeagueId)
            {
                return BadRequest(new { error = $"Team {request.TeamId.Value} does not belong to League {request.LeagueId}" });
            }
        }

        // Check authorization - must be global admin or commissioner of the league
        var isGlobalAdmin = await _authorizationService.IsGlobalAdminAsync(azureAdObjectId);
        var isCommissioner = await _authorizationService.IsCommissionerOfLeagueAsync(azureAdObjectId, request.LeagueId);

        if (!isGlobalAdmin && !isCommissioner)
        {
            return Forbid();
        }

        // Check if user already has this exact role
        var existingRole = user.LeagueRoles.FirstOrDefault(lr =>
            lr.LeagueId == request.LeagueId &&
            lr.Role.ToString() == request.Role &&
            lr.TeamId == request.TeamId &&
            !lr.IsDeleted);

        if (existingRole != null)
        {
            return BadRequest(new { error = "User already has this role in the league" });
        }

        // Create the new role
        var userRole = request.Role == "Commissioner" ? UserRole.Commissioner : UserRole.GeneralManager;
        var newRole = new UserLeagueRole
        {
            UserId = userId,
            LeagueId = request.LeagueId,
            Role = userRole,
            TeamId = request.TeamId,
            AssignedAt = DateTime.UtcNow
        };

        user.LeagueRoles.Add(newRole);
        await _userRepository.UpdateAsync(user);

        // Reload user with updated roles
        user = await _userRepository.GetByIdWithRolesAsync(userId);
        var userDto = MapToUserDto(user!);

        return Ok(userDto);
    }

    /// <summary>
    /// Removes a role from a user in a league
    /// Only global admins or league commissioners can remove roles
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="roleId">League role ID to remove</param>
    /// <returns>Updated user information</returns>
    [HttpDelete("{userId}/league-roles/{roleId}")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> RemoveLeagueRole(int userId, int roleId)
    {
        // Get current user from JWT claims
        var azureAdObjectId = HttpContext.GetAzureAdObjectId();
        if (string.IsNullOrEmpty(azureAdObjectId))
        {
            return Unauthorized(new { error = "User identity not found in token" });
        }

        // Get user with roles
        var user = await _userRepository.GetByIdWithRolesAsync(userId);
        if (user == null)
        {
            return NotFound(new { error = $"User with ID {userId} not found" });
        }

        // Find the role to remove
        var roleToRemove = user.LeagueRoles.FirstOrDefault(lr => lr.Id == roleId && !lr.IsDeleted);
        if (roleToRemove == null)
        {
            return NotFound(new { error = $"Role with ID {roleId} not found for user {userId}" });
        }

        // Check authorization - must be global admin or commissioner of the league
        var isGlobalAdmin = await _authorizationService.IsGlobalAdminAsync(azureAdObjectId);
        var isCommissioner = await _authorizationService.IsCommissionerOfLeagueAsync(azureAdObjectId, roleToRemove.LeagueId);

        if (!isGlobalAdmin && !isCommissioner)
        {
            return Forbid();
        }

        // Soft delete the role
        roleToRemove.IsDeleted = true;
        roleToRemove.DeletedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        // Reload user with updated roles
        user = await _userRepository.GetByIdWithRolesAsync(userId);
        var userDto = MapToUserDto(user!);

        return Ok(userDto);
    }

    private UserDto MapToUserDto(User user, int? filterLeagueId = null)
    {
        var roles = filterLeagueId.HasValue
            ? user.LeagueRoles.Where(lr => lr.LeagueId == filterLeagueId.Value && !lr.IsDeleted).ToList()
            : user.LeagueRoles.Where(lr => !lr.IsDeleted).ToList();

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            IsGlobalAdmin = user.IsGlobalAdmin,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            LeagueRoles = roles.Select(lr => new UserLeagueRoleDto
            {
                Id = lr.Id,
                LeagueId = lr.LeagueId,
                LeagueName = lr.League?.Name ?? string.Empty,
                Role = lr.Role.ToString(),
                TeamId = lr.TeamId,
                TeamName = lr.Team?.Name,
                AssignedAt = lr.AssignedAt
            }).ToList()
        };
    }

    private async Task<int> GetTeamLeagueIdAsync(int teamId)
    {
        var team = await _teamRepository.GetByIdAsync(teamId);
        if (team == null)
        {
            throw new InvalidOperationException($"Team {teamId} not found");
        }

        if (!team.DivisionId.HasValue)
        {
            throw new InvalidOperationException($"Team {teamId} does not have a DivisionId assigned");
        }

        // Navigate up the hierarchy: Team -> Division -> Conference -> League
        var division = await _divisionRepository.GetByIdAsync(team.DivisionId.Value);
        if (division == null)
        {
            throw new InvalidOperationException($"Division {team.DivisionId.Value} not found for team {teamId}");
        }

        var conference = await _conferenceRepository.GetByIdAsync(division.ConferenceId);
        if (conference == null)
        {
            throw new InvalidOperationException($"Conference {division.ConferenceId} not found for division {team.DivisionId.Value}");
        }

        return conference.LeagueId;
    }
}

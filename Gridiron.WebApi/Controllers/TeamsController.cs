using DataAccessLayer.Repositories;
using Gridiron.WebApi.DTOs;
using Gridiron.WebApi.Extensions;
using Gridiron.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gridiron.WebApi.Controllers;

/// <summary>
/// Controller for team and roster management
/// DOES NOT access the database directly - uses repositories from DataAccessLayer
/// REQUIRES AUTHENTICATION: All endpoints require valid Azure AD JWT token.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TeamsController : ControllerBase
{
    private readonly ITeamRepository _teamRepository;
    private readonly IGridironAuthorizationService _authorizationService;
    private readonly ILogger<TeamsController> _logger;

    public TeamsController(
        ITeamRepository teamRepository,
        IGridironAuthorizationService authorizationService,
        ILogger<TeamsController> logger)
    {
        _teamRepository = teamRepository ?? throw new ArgumentNullException(nameof(teamRepository));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all teams (filtered to only teams user has access to).
    /// </summary>
    /// <returns>List of accessible teams.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TeamDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<TeamDto>>> GetTeams()
    {
        // Get current user from JWT claims
        var azureAdObjectId = HttpContext.GetAzureAdObjectId();
        if (string.IsNullOrEmpty(azureAdObjectId))
        {
            return Unauthorized(new { error = "User identity not found in token" });
        }

        // Check if user is global admin
        var isGlobalAdmin = await _authorizationService.IsGlobalAdminAsync(azureAdObjectId);

        var teams = await _teamRepository.GetAllAsync();
        List<Team> filteredTeams;

        if (isGlobalAdmin)
        {
            // Global admins see all teams
            filteredTeams = teams;
        }
        else
        {
            // Regular users only see teams they have access to
            var accessibleTeamIds = await _authorizationService.GetAccessibleTeamIdsAsync(azureAdObjectId);
            filteredTeams = teams.Where(t => accessibleTeamIds.Contains(t.Id)).ToList();
        }

        var teamDtos = filteredTeams.Select(t => new TeamDto
        {
            Id = t.Id,
            Name = t.Name,
            City = t.City,
            Budget = t.Budget,
            Championships = t.Championships,
            Wins = t.Wins,
            Losses = t.Losses,
            Ties = t.Ties,
            FanSupport = t.FanSupport,
            Chemistry = t.Chemistry
        }).ToList();

        return Ok(teamDtos);
    }

    /// <summary>
    /// Gets a specific team by ID.
    /// </summary>
    /// <param name="id">Team ID.</param>
    /// <returns>Team details.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TeamDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TeamDto>> GetTeam(int id)
    {
        // Get current user from JWT claims
        var azureAdObjectId = HttpContext.GetAzureAdObjectId();
        if (string.IsNullOrEmpty(azureAdObjectId))
        {
            return Unauthorized(new { error = "User identity not found in token" });
        }

        // Check authorization BEFORE accessing database
        var hasAccess = await _authorizationService.CanAccessTeamAsync(azureAdObjectId, id);
        if (!hasAccess)
        {
            return Forbid();
        }

        var team = await _teamRepository.GetByIdAsync(id);

        if (team == null)
        {
            return NotFound(new { error = $"Team with ID {id} not found" });
        }

        var teamDto = new TeamDto
        {
            Id = team.Id,
            Name = team.Name,
            City = team.City,
            Budget = team.Budget,
            Championships = team.Championships,
            Wins = team.Wins,
            Losses = team.Losses,
            Ties = team.Ties,
            FanSupport = team.FanSupport,
            Chemistry = team.Chemistry
        };

        return Ok(teamDto);
    }

    /// <summary>
    /// Gets a team's roster.
    /// </summary>
    /// <param name="id">Team ID.</param>
    /// <returns>Team roster with all players.</returns>
    [HttpGet("{id}/roster")]
    [ProducesResponseType(typeof(TeamDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TeamDetailDto>> GetTeamRoster(int id)
    {
        // Get current user from JWT claims
        var azureAdObjectId = HttpContext.GetAzureAdObjectId();
        if (string.IsNullOrEmpty(azureAdObjectId))
        {
            return Unauthorized(new { error = "User identity not found in token" });
        }

        // Check authorization BEFORE accessing database
        var hasAccess = await _authorizationService.CanAccessTeamAsync(azureAdObjectId, id);
        if (!hasAccess)
        {
            return Forbid();
        }

        var team = await _teamRepository.GetByIdWithPlayersAsync(id);

        if (team == null)
        {
            return NotFound(new { error = $"Team with ID {id} not found" });
        }

        var teamDetailDto = new TeamDetailDto
        {
            Id = team.Id,
            Name = team.Name,
            City = team.City,
            Budget = team.Budget,
            Championships = team.Championships,
            Wins = team.Wins,
            Losses = team.Losses,
            Ties = team.Ties,
            FanSupport = team.FanSupport,
            Chemistry = team.Chemistry,
            Roster = team.Players.Select(p => new PlayerDto
            {
                Id = p.Id,
                FirstName = p.FirstName,
                LastName = p.LastName,
                Position = p.Position.ToString(),
                Number = p.Number,
                Height = p.Height,
                Weight = p.Weight,
                Age = p.Age,
                Exp = p.Exp,
                College = p.College,
                TeamId = p.TeamId,
                Speed = p.Speed,
                Strength = p.Strength,
                Agility = p.Agility,
                Awareness = p.Awareness,
                Morale = p.Morale,
                Discipline = p.Discipline,
                Passing = p.Passing,
                Catching = p.Catching,
                Rushing = p.Rushing,
                Blocking = p.Blocking,
                Tackling = p.Tackling,
                Coverage = p.Coverage,
                Kicking = p.Kicking,
                Health = p.Health,
                IsInjured = p.IsInjured
            }).ToList(),
            HeadCoach = team.HeadCoach != null ? new CoachDto
            {
                FirstName = team.HeadCoach.FirstName,
                LastName = team.HeadCoach.LastName
            }
            : null
        };

        return Ok(teamDetailDto);
    }
}

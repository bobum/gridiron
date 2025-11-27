using DataAccessLayer.Repositories;
using GameManagement.Services;
using Gridiron.WebApi.DTOs;
using Gridiron.WebApi.Extensions;
using Gridiron.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gridiron.WebApi.Controllers;

/// <summary>
/// Controller for team management operations (creation, roster management, depth charts)
/// DOES NOT access the database directly - uses repositories from DataAccessLayer
/// REQUIRES AUTHENTICATION: All endpoints require valid Azure AD JWT token
/// </summary>
[ApiController]
[Route("api/teams-management")]
[Authorize]
public class TeamsManagementController : ControllerBase
{
    private readonly ITeamRepository _teamRepository;
    private readonly IPlayerRepository _playerRepository;
    private readonly IDivisionRepository _divisionRepository;
    private readonly IConferenceRepository _conferenceRepository;
    private readonly ITeamBuilderService _teamBuilderService;
    private readonly IPlayerGeneratorService _playerGeneratorService;
    private readonly IGridironAuthorizationService _authorizationService;
    private readonly ILogger<TeamsManagementController> _logger;

    public TeamsManagementController(
        ITeamRepository teamRepository,
        IPlayerRepository playerRepository,
        IDivisionRepository divisionRepository,
        IConferenceRepository conferenceRepository,
        ITeamBuilderService teamBuilderService,
        IPlayerGeneratorService playerGeneratorService,
        IGridironAuthorizationService authorizationService,
        ILogger<TeamsManagementController> logger)
    {
        _teamRepository = teamRepository ?? throw new ArgumentNullException(nameof(teamRepository));
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
        _divisionRepository = divisionRepository ?? throw new ArgumentNullException(nameof(divisionRepository));
        _conferenceRepository = conferenceRepository ?? throw new ArgumentNullException(nameof(conferenceRepository));
        _teamBuilderService = teamBuilderService ?? throw new ArgumentNullException(nameof(teamBuilderService));
        _playerGeneratorService = playerGeneratorService ?? throw new ArgumentNullException(nameof(playerGeneratorService));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new team (Only Commissioners can create teams in their league)
    /// </summary>
    /// <param name="request">Team creation request</param>
    /// <returns>Created team</returns>
    [HttpPost]
    [ProducesResponseType(typeof(TeamDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TeamDto>> CreateTeam([FromBody] CreateTeamRequest request)
    {
        if (request == null)
        {
            return BadRequest(new { error = "Request body is required" });
        }

        if (string.IsNullOrWhiteSpace(request.City))
        {
            return BadRequest(new { error = "City is required" });
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { error = "Name is required" });
        }

        if (request.Budget <= 0)
        {
            return BadRequest(new { error = "Budget must be greater than 0" });
        }

        if (request.DivisionId <= 0)
        {
            return BadRequest(new { error = "DivisionId is required" });
        }

        try
        {
            // Get current user from JWT claims
            var azureAdObjectId = HttpContext.GetAzureAdObjectId();
            if (string.IsNullOrEmpty(azureAdObjectId))
            {
                return Unauthorized(new { error = "User identity not found in token" });
            }

            // Get division to determine league (need to trace division -> conference -> league)
            var division = await _divisionRepository.GetByIdAsync(request.DivisionId);
            if (division == null)
            {
                return BadRequest(new { error = $"Division with ID {request.DivisionId} not found" });
            }

            // Get conference to get leagueId (Division -> Conference -> League hierarchy)
            var conference = await _conferenceRepository.GetByIdAsync(division.ConferenceId);
            if (conference == null)
            {
                return BadRequest(new { error = "Division's conference not found" });
            }

            // SECURITY: Only Commissioners of the league can create teams
            var isCommissioner = await _authorizationService.IsCommissionerOfLeagueAsync(azureAdObjectId, conference.LeagueId);
            var isGlobalAdmin = await _authorizationService.IsGlobalAdminAsync(azureAdObjectId);

            if (!isCommissioner && !isGlobalAdmin)
            {
                return Forbid();
            }

            // Create team using TeamBuilderService
            var team = _teamBuilderService.CreateTeam(request.City, request.Name, request.Budget);
            team.DivisionId = request.DivisionId;

            // Persist to database
            await _teamRepository.AddAsync(team);

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

            _logger.LogInformation("Created team: {City} {Name} (ID: {Id})",
                team.City, team.Name, team.Id);

            return CreatedAtAction(nameof(GetTeam), new { id = team.Id }, teamDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating team");
            return StatusCode(500, new { error = "Failed to create team" });
        }
    }

    /// <summary>
    /// Gets a specific team by ID
    /// </summary>
    /// <param name="id">Team ID</param>
    /// <returns>Team details</returns>
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
    /// Adds a player to a team's roster (GM can modify their own roster, Commissioner can modify any team in their league)
    /// </summary>
    /// <param name="id">Team ID</param>
    /// <param name="request">Add player request</param>
    /// <returns>Updated team</returns>
    [HttpPut("{id}/roster")]
    [ProducesResponseType(typeof(TeamDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TeamDto>> AddPlayerToRoster(int id, [FromBody] AddPlayerToRosterRequest request)
    {
        if (request == null)
        {
            return BadRequest(new { error = "Request body is required" });
        }

        // Get current user from JWT claims
        var azureAdObjectId = HttpContext.GetAzureAdObjectId();
        if (string.IsNullOrEmpty(azureAdObjectId))
        {
            return Unauthorized(new { error = "User identity not found in token" });
        }

        // Check authorization BEFORE accessing database (GM can modify their own roster)
        var hasAccess = await _authorizationService.CanAccessTeamAsync(azureAdObjectId, id);
        if (!hasAccess)
        {
            return Forbid();
        }

        // Get team
        var team = await _teamRepository.GetByIdAsync(id);
        if (team == null)
        {
            return NotFound(new { error = $"Team with ID {id} not found" });
        }

        // Get player
        var player = await _playerRepository.GetByIdAsync(request.PlayerId);
        if (player == null)
        {
            return NotFound(new { error = $"Player with ID {request.PlayerId} not found" });
        }

        // Add player to team using TeamBuilderService
        var success = _teamBuilderService.AddPlayerToTeam(team, player);

        if (!success)
        {
            return BadRequest(new { error = "Cannot add player: Roster is full (53 players maximum)" });
        }

        // Update both team and player in database
        await _teamRepository.UpdateAsync(team);
        await _playerRepository.UpdateAsync(player);

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

        _logger.LogInformation("Added player {PlayerId} to team {TeamId} roster",
            request.PlayerId, id);

        return Ok(teamDto);
    }

    /// <summary>
    /// Builds depth charts for all units (offense, defense, special teams)
    /// </summary>
    /// <param name="id">Team ID</param>
    /// <returns>Updated team</returns>
    [HttpPost("{id}/depth-charts")]
    [ProducesResponseType(typeof(TeamDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TeamDto>> BuildDepthCharts(int id)
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

        // Get team with players
        var team = await _teamRepository.GetByIdAsync(id);
        if (team == null)
        {
            return NotFound(new { error = $"Team with ID {id} not found" });
        }

        if (team.Players == null || team.Players.Count == 0)
        {
            return BadRequest(new { error = "Cannot build depth charts: Team has no players" });
        }

        try
        {
            // Build depth charts using TeamBuilderService
            _teamBuilderService.AssignDepthCharts(team);

            // Update team in database
            await _teamRepository.UpdateAsync(team);

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

            _logger.LogInformation("Built depth charts for team {TeamId} with {PlayerCount} players",
                id, team.Players.Count);

            return Ok(teamDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building depth charts for team {TeamId}", id);
            return StatusCode(500, new { error = "Failed to build depth charts" });
        }
    }

    /// <summary>
    /// Validates a team's roster meets NFL requirements
    /// </summary>
    /// <param name="id">Team ID</param>
    /// <returns>Validation result</returns>
    [HttpGet("{id}/validate-roster")]
    [ProducesResponseType(typeof(RosterValidationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<RosterValidationResult>> ValidateRoster(int id)
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

        // Get team with players
        var team = await _teamRepository.GetByIdAsync(id);
        if (team == null)
        {
            return NotFound(new { error = $"Team with ID {id} not found" });
        }

        try
        {
            var isValid = _teamBuilderService.ValidateRoster(team);

            var result = new RosterValidationResult
            {
                TeamId = team.Id,
                TeamName = $"{team.City} {team.Name}",
                IsValid = isValid,
                PlayerCount = team.Players?.Count ?? 0
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating roster for team {TeamId}", id);
            return StatusCode(500, new { error = "Failed to validate roster" });
        }
    }

    /// <summary>
    /// Populates a team's roster with 53 randomly generated players following NFL roster composition
    /// </summary>
    /// <param name="id">Team ID</param>
    /// <param name="seed">Optional seed for reproducible generation</param>
    /// <returns>Updated team with populated roster</returns>
    [HttpPost("{id}/populate-roster")]
    [ProducesResponseType(typeof(TeamDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TeamDto>> PopulateTeamRoster(int id, [FromQuery] int? seed = null)
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

        // Get team
        var team = await _teamRepository.GetByIdAsync(id);
        if (team == null)
        {
            return NotFound(new { error = $"Team with ID {id} not found" });
        }

        try
        {
            // Populate roster using TeamBuilderService
            _teamBuilderService.PopulateTeamRoster(team, seed);

            // Update team in database
            await _teamRepository.UpdateAsync(team);

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

            _logger.LogInformation("Populated roster for team {TeamId} with 53 players (seed: {Seed})",
                id, seed?.ToString() ?? "random");

            return Ok(teamDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error populating roster for team {TeamId}", id);
            return StatusCode(500, new { error = "Failed to populate roster" });
        }
    }

    /// <summary>
    /// Updates an existing team
    /// </summary>
    /// <param name="id">Team ID</param>
    /// <param name="request">Update request with optional fields</param>
    /// <returns>Updated team</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(TeamDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TeamDto>> UpdateTeam(int id, [FromBody] UpdateTeamRequest request)
    {
        if (request == null)
        {
            return BadRequest(new { error = "Request body is required" });
        }

        try
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

            // Get team from repository (excludes soft-deleted by default)
            var team = await _teamRepository.GetByIdAsync(id);
            if (team == null)
            {
                return NotFound(new { error = $"Team with ID {id} not found" });
            }

            // Use builder service to update team (domain logic)
            _teamBuilderService.UpdateTeam(team, request.Name, request.City, request.Budget,
                request.Championships, request.Wins, request.Losses, request.Ties,
                request.FanSupport, request.Chemistry);

            // Persist changes
            await _teamRepository.UpdateAsync(team);

            // Map to DTO and return
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

            _logger.LogInformation(
                "Updated team {TeamId}: Name={Name}, City={City}",
                team.Id, team.Name, team.City);

            return Ok(teamDto);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid update request for team {TeamId}", id);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating team {TeamId}", id);
            return StatusCode(500, new { error = "Failed to update team" });
        }
    }
}

// DTOs for team management endpoints

public class CreateTeamRequest
{
    public string City { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Budget { get; set; }
    public int DivisionId { get; set; }
}

public class AddPlayerToRosterRequest
{
    public int PlayerId { get; set; }
}

public class RosterValidationResult
{
    public int TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public int PlayerCount { get; set; }
}

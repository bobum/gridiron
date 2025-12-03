using DataAccessLayer.Repositories;
using DomainObjects;
using GameManagement.Services;
using Gridiron.WebApi.DTOs;
using Gridiron.WebApi.Extensions;
using Gridiron.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gridiron.WebApi.Controllers;

/// <summary>
/// Controller for player information
/// DOES NOT access the database directly - uses repositories from DataAccessLayer
/// REQUIRES AUTHENTICATION: All endpoints require valid Azure AD JWT token.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PlayersController : ControllerBase
{
    private readonly IPlayerRepository _playerRepository;
    private readonly IPlayerGeneratorService _playerGeneratorService;
    private readonly IGridironAuthorizationService _authorizationService;
    private readonly ILogger<PlayersController> _logger;

    public PlayersController(
        IPlayerRepository playerRepository,
        IPlayerGeneratorService playerGeneratorService,
        IGridironAuthorizationService authorizationService,
        ILogger<PlayersController> logger)
    {
        _playerRepository = playerRepository ?? throw new ArgumentNullException(nameof(playerRepository));
        _playerGeneratorService = playerGeneratorService ?? throw new ArgumentNullException(nameof(playerGeneratorService));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all players (filtered to only players on teams user has access to).
    /// </summary>
    /// <returns>List of accessible players.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PlayerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<PlayerDto>>> GetPlayers([FromQuery] int? teamId = null)
    {
        // Get current user from JWT claims
        var azureAdObjectId = HttpContext.GetAzureAdObjectId();
        if (string.IsNullOrEmpty(azureAdObjectId))
        {
            return Unauthorized(new { error = "User identity not found in token" });
        }

        // If teamId is specified, check if user has access to that team
        if (teamId.HasValue)
        {
            var hasAccess = await _authorizationService.CanAccessTeamAsync(azureAdObjectId, teamId.Value);
            if (!hasAccess)
            {
                return Forbid();
            }
        }

        var players = teamId.HasValue
            ? await _playerRepository.GetByTeamIdAsync(teamId.Value)
            : await _playerRepository.GetAllAsync();

        // Filter players to only those on teams the user can access
        var accessibleTeamIds = await _authorizationService.GetAccessibleTeamIdsAsync(azureAdObjectId);
        var isGlobalAdmin = await _authorizationService.IsGlobalAdminAsync(azureAdObjectId);

        List<Player> filteredPlayers;
        if (isGlobalAdmin)
        {
            filteredPlayers = players;
        }
        else
        {
            filteredPlayers = players.Where(p => p.TeamId.HasValue && accessibleTeamIds.Contains(p.TeamId.Value)).ToList();
        }

        var playerDtos = filteredPlayers.Select(p => new PlayerDto
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
        }).ToList();

        return Ok(playerDtos);
    }

    /// <summary>
    /// Gets a specific player by ID (must be on a team the user can access).
    /// </summary>
    /// <param name="id">Player ID.</param>
    /// <returns>Player details with stats.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PlayerDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PlayerDetailDto>> GetPlayer(int id)
    {
        // Get current user from JWT claims
        var azureAdObjectId = HttpContext.GetAzureAdObjectId();
        if (string.IsNullOrEmpty(azureAdObjectId))
        {
            return Unauthorized(new { error = "User identity not found in token" });
        }

        var player = await _playerRepository.GetByIdAsync(id);

        if (player == null)
        {
            return NotFound(new { error = $"Player with ID {id} not found" });
        }

        // CRITICAL: Check if player is on a team the user can access
        if (player.TeamId.HasValue)
        {
            var hasAccess = await _authorizationService.CanAccessTeamAsync(azureAdObjectId, player.TeamId.Value);
            if (!hasAccess)
            {
                return Forbid();
            }
        }
        else
        {
            // Players not assigned to teams can only be viewed by global admins
            var isGlobalAdmin = await _authorizationService.IsGlobalAdminAsync(azureAdObjectId);
            if (!isGlobalAdmin)
            {
                return Forbid();
            }
        }

        var playerDetailDto = new PlayerDetailDto
        {
            Id = player.Id,
            FirstName = player.FirstName,
            LastName = player.LastName,
            Position = player.Position.ToString(),
            Number = player.Number,
            Height = player.Height,
            Weight = player.Weight,
            Age = player.Age,
            Exp = player.Exp,
            College = player.College,
            TeamId = player.TeamId,
            Speed = player.Speed,
            Strength = player.Strength,
            Agility = player.Agility,
            Awareness = player.Awareness,
            Morale = player.Morale,
            Discipline = player.Discipline,
            Passing = player.Passing,
            Catching = player.Catching,
            Rushing = player.Rushing,
            Blocking = player.Blocking,
            Tackling = player.Tackling,
            Coverage = player.Coverage,
            Kicking = player.Kicking,
            Health = player.Health,
            IsInjured = player.IsInjured,
            GameStats = player.Stats?.ToDictionary(
                kvp => kvp.Key.ToString(),
                kvp => kvp.Value) ?? new Dictionary<string, int>(),
            SeasonStats = player.SeasonStats?.ToDictionary(
                kvp => kvp.Key.ToString(),
                kvp => kvp.Value) ?? new Dictionary<string, int>(),
            CareerStats = player.CareerStats?.ToDictionary(
                kvp => kvp.Key.ToString(),
                kvp => kvp.Value) ?? new Dictionary<string, int>()
        };

        return Ok(playerDetailDto);
    }

    /// <summary>
    /// Generates a random player with position-specific attributes (Global admins only, or anyone can generate for scouting purposes).
    /// </summary>
    /// <param name="request">Generation request with position and optional seed.</param>
    /// <returns>Generated player.</returns>
    [HttpPost("generate")]
    [ProducesResponseType(typeof(PlayerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<PlayerDto> GenerateRandomPlayer([FromBody] GeneratePlayerRequest request)
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

        // Note: Player generation is allowed for all authenticated users (for scouting/draft purposes)
        // The generated players are not persisted to database unless added to a team roster
        if (!Enum.TryParse<Positions>(request.Position, true, out var position))
        {
            return BadRequest(new { error = $"Invalid position: {request.Position}" });
        }

        try
        {
            var player = _playerGeneratorService.GenerateRandomPlayer(position, request.Seed);

            var playerDto = new PlayerDto
            {
                Id = player.Id,
                FirstName = player.FirstName,
                LastName = player.LastName,
                Position = player.Position.ToString(),
                Number = player.Number,
                Height = player.Height,
                Weight = player.Weight,
                Age = player.Age,
                Exp = player.Exp,
                College = player.College,
                TeamId = player.TeamId,
                Speed = player.Speed,
                Strength = player.Strength,
                Agility = player.Agility,
                Awareness = player.Awareness,
                Morale = player.Morale,
                Discipline = player.Discipline,
                Passing = player.Passing,
                Catching = player.Catching,
                Rushing = player.Rushing,
                Blocking = player.Blocking,
                Tackling = player.Tackling,
                Coverage = player.Coverage,
                Kicking = player.Kicking,
                Health = player.Health,
                IsInjured = player.IsInjured
            };

            _logger.LogInformation(
                "Generated player: {FirstName} {LastName} ({Position})",
                player.FirstName, player.LastName, player.Position);

            return Ok(playerDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating player");
            return StatusCode(500, new { error = "Failed to generate player" });
        }
    }

    /// <summary>
    /// Generates a complete draft class with multiple rounds (All authenticated users can generate draft classes for scouting).
    /// </summary>
    /// <param name="request">Draft class request with year and rounds.</param>
    /// <returns>List of generated draft prospects.</returns>
    [HttpPost("draft-class")]
    [ProducesResponseType(typeof(IEnumerable<PlayerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<IEnumerable<PlayerDto>> GenerateDraftClass([FromBody] GenerateDraftClassRequest request)
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

        // Note: Draft class generation is allowed for all authenticated users
        // These are hypothetical players for scouting/draft purposes
        if (request.Year < 2000 || request.Year > 2100)
        {
            return BadRequest(new { error = "Year must be between 2000 and 2100" });
        }

        if (request.Rounds < 1 || request.Rounds > 10)
        {
            return BadRequest(new { error = "Rounds must be between 1 and 10" });
        }

        try
        {
            var draftClass = _playerGeneratorService.GenerateDraftClass(request.Year, request.Rounds);

            var playerDtos = draftClass.Select(player => new PlayerDto
            {
                Id = player.Id,
                FirstName = player.FirstName,
                LastName = player.LastName,
                Position = player.Position.ToString(),
                Number = player.Number,
                Height = player.Height,
                Weight = player.Weight,
                Age = player.Age,
                Exp = player.Exp,
                College = player.College,
                TeamId = player.TeamId,
                Speed = player.Speed,
                Strength = player.Strength,
                Agility = player.Agility,
                Awareness = player.Awareness,
                Morale = player.Morale,
                Discipline = player.Discipline,
                Passing = player.Passing,
                Catching = player.Catching,
                Rushing = player.Rushing,
                Blocking = player.Blocking,
                Tackling = player.Tackling,
                Coverage = player.Coverage,
                Kicking = player.Kicking,
                Health = player.Health,
                IsInjured = player.IsInjured
            }).ToList();

            _logger.LogInformation(
                "Generated draft class: {Year} with {Count} prospects ({Rounds} rounds)",
                request.Year, playerDtos.Count, request.Rounds);

            return Ok(playerDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating draft class");
            return StatusCode(500, new { error = "Failed to generate draft class" });
        }
    }
}

// DTOs for player generation endpoints
public class GeneratePlayerRequest
{
    public string Position { get; set; } = string.Empty;

    public int? Seed { get; set; }
}

public class GenerateDraftClassRequest
{
    public int Year { get; set; }

    public int Rounds { get; set; } = 7;
}

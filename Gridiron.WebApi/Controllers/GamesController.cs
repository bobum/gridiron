using Gridiron.WebApi.DTOs;
using Gridiron.WebApi.Extensions;
using Gridiron.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gridiron.WebApi.Controllers;

/// <summary>
/// Controller for game simulation and results
/// REQUIRES AUTHENTICATION: All endpoints require valid Azure AD JWT token
/// Games are viewable by anyone in the league (both teams' GMs + Commissioner).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GamesController : ControllerBase
{
    private readonly IGameSimulationService _simulationService;
    private readonly IGridironAuthorizationService _authorizationService;
    private readonly ILogger<GamesController> _logger;

    public GamesController(
        IGameSimulationService simulationService,
        IGridironAuthorizationService authorizationService,
        ILogger<GamesController> logger)
    {
        _simulationService = simulationService ?? throw new ArgumentNullException(nameof(simulationService));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Simulates a new game (User must have access to at least one of the teams involved).
    /// </summary>
    /// <param name="request">Game simulation request.</param>
    /// <returns>Completed game result.</returns>
    [HttpPost("simulate")]
    [ProducesResponseType(typeof(GameDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<GameDto>> SimulateGame([FromBody] SimulateGameRequest request)
    {
        try
        {
            // Get current user from JWT claims
            var azureAdObjectId = HttpContext.GetAzureAdObjectId();
            if (string.IsNullOrEmpty(azureAdObjectId))
            {
                return Unauthorized(new { error = "User identity not found in token" });
            }

            // Check if user has access to at least one of the teams
            var hasAccessToHome = await _authorizationService.CanAccessTeamAsync(azureAdObjectId, request.HomeTeamId);
            var hasAccessToAway = await _authorizationService.CanAccessTeamAsync(azureAdObjectId, request.AwayTeamId);

            if (!hasAccessToHome && !hasAccessToAway)
            {
                return Forbid();
            }

            _logger.LogInformation(
                "Simulating game: Home={HomeTeamId}, Away={AwayTeamId}",
                request.HomeTeamId, request.AwayTeamId);

            var game = await _simulationService.SimulateGameAsync(
                request.HomeTeamId,
                request.AwayTeamId,
                request.RandomSeed);

            var gameDto = new GameDto
            {
                Id = game.Id,
                HomeTeamId = game.HomeTeamId,
                AwayTeamId = game.AwayTeamId,
                HomeTeamName = $"{game.HomeTeam.City} {game.HomeTeam.Name}",
                AwayTeamName = $"{game.AwayTeam.City} {game.AwayTeam.Name}",
                HomeScore = game.HomeScore,
                AwayScore = game.AwayScore,
                RandomSeed = game.RandomSeed,
                IsComplete = true,
                TotalPlays = 0
            };

            return Ok(gameDto);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid game simulation request");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error simulating game");
            return StatusCode(500, new { error = "An error occurred while simulating the game" });
        }
    }

    /// <summary>
    /// Gets all simulated games (filtered to games involving teams user has access to).
    /// </summary>
    /// <returns>List of accessible games.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<GameDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IEnumerable<GameDto>>> GetGames()
    {
        // Get current user from JWT claims
        var azureAdObjectId = HttpContext.GetAzureAdObjectId();
        if (string.IsNullOrEmpty(azureAdObjectId))
        {
            return Unauthorized(new { error = "User identity not found in token" });
        }

        var games = await _simulationService.GetGamesAsync();

        // Filter to games where user has access to at least one team
        var accessibleTeamIds = await _authorizationService.GetAccessibleTeamIdsAsync(azureAdObjectId);
        var isGlobalAdmin = await _authorizationService.IsGlobalAdminAsync(azureAdObjectId);

        List<DomainObjects.Game> filteredGames;
        if (isGlobalAdmin)
        {
            filteredGames = games;
        }
        else
        {
            filteredGames = games.Where(g =>
                accessibleTeamIds.Contains(g.HomeTeamId) ||
                accessibleTeamIds.Contains(g.AwayTeamId))
            .ToList();
        }

        var gameDtos = filteredGames.Select(g => new GameDto
        {
            Id = g.Id,
            HomeTeamId = g.HomeTeamId,
            AwayTeamId = g.AwayTeamId,
            HomeTeamName = $"{g.HomeTeam.City} {g.HomeTeam.Name}",
            AwayTeamName = $"{g.AwayTeam.City} {g.AwayTeam.Name}",
            HomeScore = g.HomeScore,
            AwayScore = g.AwayScore,
            RandomSeed = g.RandomSeed,
            IsComplete = true,
            TotalPlays = 0
        }).ToList();

        return Ok(gameDtos);
    }

    /// <summary>
    /// Gets a specific game by ID (User must have access to at least one of the teams).
    /// </summary>
    /// <param name="id">Game ID.</param>
    /// <returns>Game details.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(GameDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<GameDto>> GetGame(int id)
    {
        // Get current user from JWT claims
        var azureAdObjectId = HttpContext.GetAzureAdObjectId();
        if (string.IsNullOrEmpty(azureAdObjectId))
        {
            return Unauthorized(new { error = "User identity not found in token" });
        }

        var game = await _simulationService.GetGameAsync(id);

        if (game == null)
        {
            return NotFound(new { error = $"Game with ID {id} not found" });
        }

        // Check if user has access to at least one of the teams in the game
        var hasAccessToHome = await _authorizationService.CanAccessTeamAsync(azureAdObjectId, game.HomeTeamId);
        var hasAccessToAway = await _authorizationService.CanAccessTeamAsync(azureAdObjectId, game.AwayTeamId);

        if (!hasAccessToHome && !hasAccessToAway)
        {
            return Forbid();
        }

        var gameDto = new GameDto
        {
            Id = game.Id,
            HomeTeamId = game.HomeTeamId,
            AwayTeamId = game.AwayTeamId,
            HomeTeamName = $"{game.HomeTeam.City} {game.HomeTeam.Name}",
            AwayTeamName = $"{game.AwayTeam.City} {game.AwayTeam.Name}",
            HomeScore = game.HomeScore,
            AwayScore = game.AwayScore,
            RandomSeed = game.RandomSeed,
            IsComplete = true,
            TotalPlays = 0
        };

        return Ok(gameDto);
    }

    /// <summary>
    /// Gets play-by-play data for a game (User must have access to at least one of the teams).
    /// </summary>
    /// <param name="id">Game ID.</param>
    /// <returns>List of plays.</returns>
    [HttpGet("{id}/plays")]
    [ProducesResponseType(typeof(IEnumerable<PlayDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<PlayDto>>> GetGamePlays(int id)
    {
        // Get current user from JWT claims
        var azureAdObjectId = HttpContext.GetAzureAdObjectId();
        if (string.IsNullOrEmpty(azureAdObjectId))
        {
            return Unauthorized(new { error = "User identity not found in token" });
        }

        // First check if game exists and user has access
        var game = await _simulationService.GetGameAsync(id);

        if (game == null)
        {
            return NotFound(new { error = $"Game with ID {id} not found" });
        }

        // Check if user has access to at least one of the teams in the game
        var hasAccessToHome = await _authorizationService.CanAccessTeamAsync(azureAdObjectId, game.HomeTeamId);
        var hasAccessToAway = await _authorizationService.CanAccessTeamAsync(azureAdObjectId, game.AwayTeamId);

        if (!hasAccessToHome && !hasAccessToAway)
        {
            return Forbid();
        }

        // Get plays from PlayByPlay.PlaysJson (deserialized by the service)
        var plays = await _simulationService.GetGamePlaysAsync(id);

        return Ok(plays ?? new List<PlayDto>());
    }
}

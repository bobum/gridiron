using Gridiron.WebApi.DTOs;
using Gridiron.WebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace Gridiron.WebApi.Controllers;

/// <summary>
/// Controller for game simulation and results
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class GamesController : ControllerBase
{
    private readonly IGameSimulationService _simulationService;
    private readonly ILogger<GamesController> _logger;

    public GamesController(
        IGameSimulationService simulationService,
        ILogger<GamesController> logger)
    {
        _simulationService = simulationService ?? throw new ArgumentNullException(nameof(simulationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Simulates a new game
    /// </summary>
    /// <param name="request">Game simulation request</param>
    /// <returns>Completed game result</returns>
    [HttpPost("simulate")]
    [ProducesResponseType(typeof(GameDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GameDto>> SimulateGame([FromBody] SimulateGameRequest request)
    {
        try
        {
            _logger.LogInformation("Simulating game: Home={HomeTeamId}, Away={AwayTeamId}",
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
                TotalPlays = game.Plays?.Count ?? 0
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
    /// Gets all simulated games
    /// </summary>
    /// <returns>List of all games</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<GameDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<GameDto>>> GetGames()
    {
        var games = await _simulationService.GetGamesAsync();

        var gameDtos = games.Select(g => new GameDto
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
            TotalPlays = g.Plays?.Count ?? 0
        }).ToList();

        return Ok(gameDtos);
    }

    /// <summary>
    /// Gets a specific game by ID
    /// </summary>
    /// <param name="id">Game ID</param>
    /// <returns>Game details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(GameDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GameDto>> GetGame(int id)
    {
        var game = await _simulationService.GetGameAsync(id);

        if (game == null)
        {
            return NotFound(new { error = $"Game with ID {id} not found" });
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
            TotalPlays = game.Plays?.Count ?? 0
        };

        return Ok(gameDto);
    }

    /// <summary>
    /// Gets play-by-play data for a game
    /// </summary>
    /// <param name="id">Game ID</param>
    /// <returns>List of plays</returns>
    [HttpGet("{id}/plays")]
    [ProducesResponseType(typeof(IEnumerable<PlayDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<PlayDto>>> GetGamePlays(int id)
    {
        var game = await _simulationService.GetGameAsync(id);

        if (game == null)
        {
            return NotFound(new { error = $"Game with ID {id} not found" });
        }

        if (game.Plays == null || !game.Plays.Any())
        {
            return Ok(new List<PlayDto>());
        }

        var playDtos = game.Plays.Select(p => new PlayDto
        {
            PlayType = p.PlayType.ToString(),
            Possession = p.Possession.ToString(),
            Down = (int)p.Down,
            YardsToGo = 0, // Will need to calculate from game state
            StartFieldPosition = p.StartFieldPosition,
            EndFieldPosition = p.EndFieldPosition,
            YardsGained = p.YardsGained,
            StartTime = p.StartTime,
            StopTime = p.StopTime,
            ElapsedTime = p.ElapsedTime,
            IsTouchdown = p.IsTouchdown,
            IsSafety = p.IsSafety,
            Interception = p.Interception,
            PossessionChange = p.PossessionChange,
            Penalties = p.Penalties?.Select(pen => pen.ToString()).ToList() ?? new List<string>(),
            Fumbles = p.Fumbles?.Select(f => f.ToString()).ToList() ?? new List<string>(),
            Injuries = p.Injuries?.Select(i => i.ToString()).ToList() ?? new List<string>(),
            Description = GeneratePlayDescription(p)
        }).ToList();

        return Ok(playDtos);
    }

    private string GeneratePlayDescription(DomainObjects.IPlay play)
    {
        // Basic play description - can be enhanced later
        var desc = $"{play.PlayType} play: ";

        if (play.IsTouchdown)
            desc += "TOUCHDOWN! ";
        else if (play.IsSafety)
            desc += "SAFETY! ";
        else if (play.Interception)
            desc += "INTERCEPTION! ";

        desc += $"{play.YardsGained} yards";

        if (play.Penalties?.Any() == true)
            desc += $" (Penalty: {play.Penalties.Count})";

        return desc;
    }
}

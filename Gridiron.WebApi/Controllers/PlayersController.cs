using DataAccessLayer;
using Gridiron.WebApi.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gridiron.WebApi.Controllers;

/// <summary>
/// Controller for player information
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PlayersController : ControllerBase
{
    private readonly GridironDbContext _dbContext;
    private readonly ILogger<PlayersController> _logger;

    public PlayersController(
        GridironDbContext dbContext,
        ILogger<PlayersController> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all players
    /// </summary>
    /// <returns>List of all players</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PlayerDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PlayerDto>>> GetPlayers([FromQuery] int? teamId = null)
    {
        var query = _dbContext.Players.AsQueryable();

        if (teamId.HasValue)
        {
            query = query.Where(p => p.TeamId == teamId.Value);
        }

        var players = await query.ToListAsync();

        var playerDtos = players.Select(p => new PlayerDto
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
    /// Gets a specific player by ID
    /// </summary>
    /// <param name="id">Player ID</param>
    /// <returns>Player details with stats</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PlayerDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlayerDetailDto>> GetPlayer(int id)
    {
        var player = await _dbContext.Players.FindAsync(id);

        if (player == null)
        {
            return NotFound(new { error = $"Player with ID {id} not found" });
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
}

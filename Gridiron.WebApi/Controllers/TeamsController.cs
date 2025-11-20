using DataAccessLayer;
using Gridiron.WebApi.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gridiron.WebApi.Controllers;

/// <summary>
/// Controller for team and roster management
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TeamsController : ControllerBase
{
    private readonly GridironDbContext _dbContext;
    private readonly ILogger<TeamsController> _logger;

    public TeamsController(
        GridironDbContext dbContext,
        ILogger<TeamsController> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all teams
    /// </summary>
    /// <returns>List of all teams</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TeamDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TeamDto>>> GetTeams()
    {
        var teams = await _dbContext.Teams.ToListAsync();

        var teamDtos = teams.Select(t => new TeamDto
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
    /// Gets a specific team by ID
    /// </summary>
    /// <param name="id">Team ID</param>
    /// <returns>Team details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TeamDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TeamDto>> GetTeam(int id)
    {
        var team = await _dbContext.Teams.FindAsync(id);

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
    /// Gets a team's roster
    /// </summary>
    /// <param name="id">Team ID</param>
    /// <returns>Team roster with all players</returns>
    [HttpGet("{id}/roster")]
    [ProducesResponseType(typeof(TeamDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TeamDetailDto>> GetTeamRoster(int id)
    {
        var team = await _dbContext.Teams
            .Include(t => t.Players)
            .FirstOrDefaultAsync(t => t.Id == id);

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
            } : null
        };

        return Ok(teamDetailDto);
    }
}

using DataAccessLayer.Repositories;
using GameManagement.Services;
using Gridiron.WebApi.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Gridiron.WebApi.Controllers;

/// <summary>
/// Controller for league management operations (creation, structure management)
/// DOES NOT access the database directly - uses repositories from DataAccessLayer
/// </summary>
[ApiController]
[Route("api/leagues-management")]
public class LeaguesManagementController : ControllerBase
{
    private readonly ILeagueRepository _leagueRepository;
    private readonly ILeagueBuilderService _leagueBuilderService;
    private readonly ILogger<LeaguesManagementController> _logger;

    public LeaguesManagementController(
        ILeagueRepository leagueRepository,
        ILeagueBuilderService leagueBuilderService,
        ILogger<LeaguesManagementController> logger)
    {
        _leagueRepository = leagueRepository ?? throw new ArgumentNullException(nameof(leagueRepository));
        _leagueBuilderService = leagueBuilderService ?? throw new ArgumentNullException(nameof(leagueBuilderService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new league with specified structure
    /// </summary>
    /// <param name="request">League creation request</param>
    /// <returns>Created league with full structure</returns>
    [HttpPost]
    [ProducesResponseType(typeof(LeagueDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LeagueDetailDto>> CreateLeague([FromBody] CreateLeagueRequest request)
    {
        if (request == null)
        {
            return BadRequest(new { error = "Request body is required" });
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(new { error = "League name is required" });
        }

        if (request.NumberOfConferences <= 0)
        {
            return BadRequest(new { error = "Number of conferences must be greater than 0" });
        }

        if (request.DivisionsPerConference <= 0)
        {
            return BadRequest(new { error = "Divisions per conference must be greater than 0" });
        }

        if (request.TeamsPerDivision <= 0)
        {
            return BadRequest(new { error = "Teams per division must be greater than 0" });
        }

        try
        {
            // Create league using LeagueBuilderService
            var league = _leagueBuilderService.CreateLeague(
                request.Name,
                request.NumberOfConferences,
                request.DivisionsPerConference,
                request.TeamsPerDivision);

            // Persist to database
            await _leagueRepository.AddAsync(league);

            // Map to detailed DTO with full structure
            var leagueDetailDto = new LeagueDetailDto
            {
                Id = league.Id,
                Name = league.Name,
                Season = league.Season,
                IsActive = league.IsActive,
                TotalConferences = league.Conferences.Count,
                TotalTeams = league.Conferences
                    .SelectMany(c => c.Divisions)
                    .SelectMany(d => d.Teams)
                    .Count(),
                Conferences = league.Conferences.Select(c => new ConferenceDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Divisions = c.Divisions.Select(d => new DivisionDto
                    {
                        Id = d.Id,
                        Name = d.Name,
                        Teams = d.Teams.Select(t => new TeamDto
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
                        }).ToList()
                    }).ToList()
                }).ToList()
            };

            _logger.LogInformation(
                "Created league: {Name} (ID: {Id}) with {Conferences} conferences and {Teams} total teams",
                league.Name, league.Id, leagueDetailDto.TotalConferences, leagueDetailDto.TotalTeams);

            return CreatedAtAction(nameof(GetLeague), new { id = league.Id }, leagueDetailDto);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid league creation request");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating league");
            return StatusCode(500, new { error = "Failed to create league" });
        }
    }

    /// <summary>
    /// Gets a specific league by ID with full structure
    /// </summary>
    /// <param name="id">League ID</param>
    /// <returns>League with full structure</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(LeagueDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LeagueDetailDto>> GetLeague(int id)
    {
        try
        {
            var league = await _leagueRepository.GetByIdWithFullStructureAsync(id);

            if (league == null)
            {
                return NotFound(new { error = $"League with ID {id} not found" });
            }

            var leagueDetailDto = new LeagueDetailDto
            {
                Id = league.Id,
                Name = league.Name,
                Season = league.Season,
                IsActive = league.IsActive,
                TotalConferences = league.Conferences.Count,
                TotalTeams = league.Conferences
                    .SelectMany(c => c.Divisions)
                    .SelectMany(d => d.Teams)
                    .Count(),
                Conferences = league.Conferences.Select(c => new ConferenceDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Divisions = c.Divisions.Select(d => new DivisionDto
                    {
                        Id = d.Id,
                        Name = d.Name,
                        Teams = d.Teams.Select(t => new TeamDto
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
                        }).ToList()
                    }).ToList()
                }).ToList()
            };

            return Ok(leagueDetailDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving league {Id}", id);
            return StatusCode(500, new { error = "Failed to retrieve league" });
        }
    }

    /// <summary>
    /// Gets all leagues (without full structure)
    /// </summary>
    /// <returns>List of all leagues</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<LeagueDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<LeagueDto>>> GetAllLeagues()
    {
        try
        {
            var leagues = await _leagueRepository.GetAllAsync();

            var leagueDtos = leagues.Select(l => new LeagueDto
            {
                Id = l.Id,
                Name = l.Name,
                Season = l.Season,
                IsActive = l.IsActive,
                TotalConferences = 0,  // Not loaded for list view
                TotalTeams = 0         // Not loaded for list view
            }).ToList();

            return Ok(leagueDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all leagues");
            return StatusCode(500, new { error = "Failed to retrieve leagues" });
        }
    }

    /// <summary>
    /// Populates rosters for all teams in a league with 53 randomly generated players each
    /// </summary>
    /// <param name="id">League ID</param>
    /// <param name="seed">Optional seed for reproducible generation</param>
    /// <returns>Updated league with all teams populated</returns>
    [HttpPost("{id}/populate-rosters")]
    [ProducesResponseType(typeof(LeagueDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<LeagueDto>> PopulateLeagueRosters(int id, [FromQuery] int? seed = null)
    {
        try
        {
            // Get league with full structure
            var league = await _leagueRepository.GetByIdWithFullStructureAsync(id);
            if (league == null)
            {
                return NotFound(new { error = $"League with ID {id} not found" });
            }

            // Populate all team rosters using LeagueBuilderService
            _leagueBuilderService.PopulateLeagueRosters(league, seed);

            // Update league in database (cascades to all teams and players)
            await _leagueRepository.UpdateAsync(league);

            var leagueDto = new LeagueDto
            {
                Id = league.Id,
                Name = league.Name,
                Season = league.Season,
                IsActive = league.IsActive,
                TotalConferences = league.Conferences.Count,
                TotalTeams = league.Conferences
                    .SelectMany(c => c.Divisions)
                    .SelectMany(d => d.Teams)
                    .Count()
            };

            _logger.LogInformation(
                "Populated rosters for all {TeamCount} teams in league {LeagueId} (seed: {Seed})",
                leagueDto.TotalTeams, id, seed?.ToString() ?? "random");

            return Ok(leagueDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error populating rosters for league {LeagueId}", id);
            return StatusCode(500, new { error = "Failed to populate league rosters" });
        }
    }
}

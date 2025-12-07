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
/// Controller for season management operations (creation, schedule generation, advancement)
/// REQUIRES AUTHENTICATION: All endpoints require valid Azure AD JWT token.
/// </summary>
[ApiController]
[Route("api/seasons")]
[Authorize]
public class SeasonsController : ControllerBase
{
    private readonly ISeasonRepository _seasonRepository;
    private readonly ILeagueRepository _leagueRepository;
    private readonly IScheduleGeneratorService _scheduleGeneratorService;
    private readonly ISeasonSimulationService _seasonSimulationService;
    private readonly IGridironAuthorizationService _authorizationService;
    private readonly ILogger<SeasonsController> _logger;

    public SeasonsController(
        ISeasonRepository seasonRepository,
        ILeagueRepository leagueRepository,
        IScheduleGeneratorService scheduleGeneratorService,
        ISeasonSimulationService seasonSimulationService,
        IGridironAuthorizationService authorizationService,
        ILogger<SeasonsController> logger)
    {
        _seasonRepository = seasonRepository ?? throw new ArgumentNullException(nameof(seasonRepository));
        _leagueRepository = leagueRepository ?? throw new ArgumentNullException(nameof(leagueRepository));
        _scheduleGeneratorService = scheduleGeneratorService ?? throw new ArgumentNullException(nameof(scheduleGeneratorService));
        _seasonSimulationService = seasonSimulationService ?? throw new ArgumentNullException(nameof(seasonSimulationService));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a specific season by ID.
    /// </summary>
    /// <param name="id">Season ID.</param>
    /// <returns>Season details.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SeasonDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SeasonDetailDto>> GetSeason(int id)
    {
        try
        {
            var season = await _seasonRepository.GetByIdWithWeeksAndGamesAsync(id);
            if (season == null)
            {
                return NotFound(new { error = $"Season with ID {id} not found" });
            }

            // Check authorization
            var azureAdObjectId = HttpContext.GetAzureAdObjectId();
            if (string.IsNullOrEmpty(azureAdObjectId))
            {
                return Unauthorized(new { error = "User identity not found in token" });
            }

            var hasAccess = await _authorizationService.CanAccessLeagueAsync(azureAdObjectId, season.LeagueId);
            if (!hasAccess)
            {
                return Forbid();
            }

            return Ok(MapToSeasonDetailDto(season));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving season {SeasonId}", id);
            return StatusCode(500, new { error = "Failed to retrieve season" });
        }
    }

    /// <summary>
    /// Gets all seasons for a league.
    /// </summary>
    /// <param name="leagueId">League ID.</param>
    /// <returns>List of seasons.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<SeasonDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<SeasonDto>>> GetSeasons([FromQuery] int leagueId)
    {
        try
        {
            // Check authorization
            var azureAdObjectId = HttpContext.GetAzureAdObjectId();
            if (string.IsNullOrEmpty(azureAdObjectId))
            {
                return Unauthorized(new { error = "User identity not found in token" });
            }

            var hasAccess = await _authorizationService.CanAccessLeagueAsync(azureAdObjectId, leagueId);
            if (!hasAccess)
            {
                return Forbid();
            }

            var seasons = await _seasonRepository.GetByLeagueIdAsync(leagueId);
            var seasonDtos = seasons.Select(MapToSeasonDto).ToList();

            return Ok(seasonDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving seasons for league {LeagueId}", leagueId);
            return StatusCode(500, new { error = "Failed to retrieve seasons" });
        }
    }

    /// <summary>
    /// Gets the current week for a season.
    /// </summary>
    /// <param name="id">Season ID.</param>
    /// <returns>Current week details.</returns>
    [HttpGet("{id}/current-week")]
    [ProducesResponseType(typeof(CurrentWeekDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CurrentWeekDto>> GetCurrentWeek(int id)
    {
        try
        {
            var season = await _seasonRepository.GetByIdWithCurrentWeekAsync(id);
            if (season == null)
            {
                return NotFound(new { error = $"Season with ID {id} not found" });
            }

            // Check authorization
            var azureAdObjectId = HttpContext.GetAzureAdObjectId();
            if (string.IsNullOrEmpty(azureAdObjectId))
            {
                return Unauthorized(new { error = "User identity not found in token" });
            }

            var hasAccess = await _authorizationService.CanAccessLeagueAsync(azureAdObjectId, season.LeagueId);
            if (!hasAccess)
            {
                return Forbid();
            }

            var currentWeek = season.Weeks.FirstOrDefault(w => w.WeekNumber == season.CurrentWeek);
            
            // Handle case where season is complete (CurrentWeek might be > TotalWeeks)
            if (currentWeek == null && season.IsComplete)
            {
                // If season is complete, show the last week
                currentWeek = season.Weeks.OrderByDescending(w => w.WeekNumber).FirstOrDefault();
            }

            if (currentWeek == null)
            {
                return NotFound(new { error = $"Current week {season.CurrentWeek} not found in season {id}" });
            }

            var dto = new CurrentWeekDto
            {
                WeekNumber = currentWeek.WeekNumber,
                Status = currentWeek.Status.ToString(),
                Phase = currentWeek.Phase.ToString(),
                Games = currentWeek.Games.Select(g => new GameDto
                {
                    Id = g.Id,
                    HomeTeamId = g.HomeTeamId,
                    AwayTeamId = g.AwayTeamId,
                    HomeTeamName = g.HomeTeam?.Name ?? "Unknown",
                    AwayTeamName = g.AwayTeam?.Name ?? "Unknown",
                    HomeScore = g.HomeScore,
                    AwayScore = g.AwayScore,
                    IsComplete = g.IsComplete,
                    RandomSeed = g.RandomSeed
                }).ToList()
            };

            return Ok(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current week for season {SeasonId}", id);
            return StatusCode(500, new { error = "Failed to retrieve current week" });
        }
    }

    /// <summary>
    /// Creates a new season for a league.
    /// </summary>
    /// <param name="leagueId">League ID.</param>
    /// <param name="request">Season creation request.</param>
    /// <returns>Created season.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(SeasonDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SeasonDto>> CreateSeason([FromQuery] int leagueId, [FromBody] CreateSeasonRequest request)
    {
        try
        {
            // Check authorization - must be Commissioner
            var azureAdObjectId = HttpContext.GetAzureAdObjectId();
            if (string.IsNullOrEmpty(azureAdObjectId))
            {
                return Unauthorized(new { error = "User identity not found in token" });
            }

            var isCommissioner = await _authorizationService.IsCommissionerOfLeagueAsync(azureAdObjectId, leagueId);
            var isGlobalAdmin = await _authorizationService.IsGlobalAdminAsync(azureAdObjectId);

            if (!isCommissioner && !isGlobalAdmin)
            {
                return Forbid();
            }

            // Verify league exists
            var league = await _leagueRepository.GetByIdAsync(leagueId);
            if (league == null)
            {
                return NotFound(new { error = $"League with ID {leagueId} not found" });
            }

            // Create the season
            var season = new Season
            {
                LeagueId = leagueId,
                Year = request.Year,
                RegularSeasonWeeks = request.RegularSeasonWeeks ?? 17,
                Phase = SeasonPhase.Preseason,
                CurrentWeek = 1,
                IsComplete = false
            };

            await _seasonRepository.AddAsync(season);

            _logger.LogInformation(
                "Created season {SeasonId} for league {LeagueId}, year {Year}",
                season.Id, leagueId, request.Year);

            return CreatedAtAction(nameof(GetSeason), new { id = season.Id }, MapToSeasonDto(season));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating season for league {LeagueId}", leagueId);
            return StatusCode(500, new { error = "Failed to create season" });
        }
    }

    /// <summary>
    /// Generates an NFL-style schedule for a season.
    /// </summary>
    /// <param name="id">Season ID.</param>
    /// <param name="request">Schedule generation request with optional seed.</param>
    /// <returns>Generated schedule details.</returns>
    [HttpPost("{id}/generate-schedule")]
    [ProducesResponseType(typeof(GenerateScheduleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GenerateScheduleResponse>> GenerateSchedule(int id, [FromBody] GenerateScheduleRequest? request)
    {
        try
        {
            // Get season with league structure
            var season = await _seasonRepository.GetByIdWithWeeksAsync(id);
            if (season == null)
            {
                return NotFound(new { error = $"Season with ID {id} not found" });
            }

            // Check authorization - must be Commissioner
            var azureAdObjectId = HttpContext.GetAzureAdObjectId();
            if (string.IsNullOrEmpty(azureAdObjectId))
            {
                return Unauthorized(new { error = "User identity not found in token" });
            }

            var isCommissioner = await _authorizationService.IsCommissionerOfLeagueAsync(azureAdObjectId, season.LeagueId);
            var isGlobalAdmin = await _authorizationService.IsGlobalAdminAsync(azureAdObjectId);

            if (!isCommissioner && !isGlobalAdmin)
            {
                return Forbid();
            }

            // Check if schedule already exists
            if (season.Weeks.Any(w => w.Games.Count > 0))
            {
                return BadRequest(new { error = "Season already has a schedule. Delete existing games first." });
            }

            // Load league with full structure for schedule generation
            var league = await _leagueRepository.GetByIdWithFullStructureAsync(season.LeagueId);
            if (league == null)
            {
                return NotFound(new { error = $"League with ID {season.LeagueId} not found" });
            }

            season.League = league;

            // Validate league structure
            var validation = _scheduleGeneratorService.ValidateLeagueStructure(league);
            if (!validation.IsValid)
            {
                return BadRequest(new
                {
                    error = "League structure is not valid for schedule generation",
                    errors = validation.Errors,
                    warnings = validation.Warnings
                });
            }

            // Generate the schedule
            _scheduleGeneratorService.GenerateSchedule(season, request?.Seed);

            // Save the season with generated weeks and games
            await _seasonRepository.UpdateAsync(season);
            await _seasonRepository.SaveChangesAsync();

            // Reload to get IDs
            season = await _seasonRepository.GetByIdWithWeeksAndGamesAsync(id);

            var totalGames = season!.Weeks.Sum(w => w.Games.Count);
            var teamCount = league.Conferences
                .SelectMany(c => c.Divisions)
                .SelectMany(d => d.Teams)
                .Count();

            _logger.LogInformation(
                "Generated schedule for season {SeasonId}: {TotalGames} games over {Weeks} weeks",
                id, totalGames, season.Weeks.Count);

            // Load teams for response mapping
            var fullSeason = await _seasonRepository.GetByIdWithFullDataAsync(id);

            return Ok(new GenerateScheduleResponse
            {
                SeasonId = id,
                TotalWeeks = season.Weeks.Count,
                TotalGames = totalGames,
                GamesPerTeam = teamCount > 0 ? totalGames * 2 / teamCount : 0,
                Warnings = validation.Warnings,
                Season = MapToSeasonDetailDto(fullSeason!)
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation generating schedule for season {SeasonId}", id);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating schedule for season {SeasonId}", id);
            return StatusCode(500, new { error = "Failed to generate schedule" });
        }
    }

    /// <summary>
    /// Gets the schedule for a season (all weeks with games).
    /// </summary>
    /// <param name="id">Season ID.</param>
    /// <returns>Schedule with all weeks and games.</returns>
    [HttpGet("{id}/schedule")]
    [ProducesResponseType(typeof(List<SeasonWeekDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<List<SeasonWeekDto>>> GetSchedule(int id)
    {
        try
        {
            var season = await _seasonRepository.GetByIdWithFullDataAsync(id);
            if (season == null)
            {
                return NotFound(new { error = $"Season with ID {id} not found" });
            }

            // Check authorization
            var azureAdObjectId = HttpContext.GetAzureAdObjectId();
            if (string.IsNullOrEmpty(azureAdObjectId))
            {
                return Unauthorized(new { error = "User identity not found in token" });
            }

            var hasAccess = await _authorizationService.CanAccessLeagueAsync(azureAdObjectId, season.LeagueId);
            if (!hasAccess)
            {
                return Forbid();
            }

            var weekDtos = season.Weeks
                .OrderBy(w => w.WeekNumber)
                .Select(MapToSeasonWeekDto)
                .ToList();

            return Ok(weekDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving schedule for season {SeasonId}", id);
            return StatusCode(500, new { error = "Failed to retrieve schedule" });
        }
    }

    /// <summary>
    /// Advances the season by simulating all unplayed games in the current week.
    /// </summary>
    /// <param name="id">Season ID.</param>
    /// <returns>Simulation results for the week.</returns>
    [HttpPost("{id}/advance-week")]
    [ProducesResponseType(typeof(SeasonSimulationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SeasonSimulationResult>> AdvanceWeek(int id)
    {
        try
        {
            var season = await _seasonRepository.GetByIdAsync(id);
            if (season == null)
            {
                return NotFound(new { error = $"Season with ID {id} not found" });
            }

            // Check authorization - must be Commissioner
            var azureAdObjectId = HttpContext.GetAzureAdObjectId();
            if (string.IsNullOrEmpty(azureAdObjectId))
            {
                return Unauthorized(new { error = "User identity not found in token" });
            }

            var isCommissioner = await _authorizationService.IsCommissionerOfLeagueAsync(azureAdObjectId, season.LeagueId);
            var isGlobalAdmin = await _authorizationService.IsGlobalAdminAsync(azureAdObjectId);

            if (!isCommissioner && !isGlobalAdmin)
            {
                return Forbid();
            }

            var result = await _seasonSimulationService.SimulateCurrentWeekAsync(id);

            if (!result.Success)
            {
                return BadRequest(new { error = result.Error });
            }

            _logger.LogInformation(
                "Advanced season {SeasonId} to week {WeekNumber}. Simulated {GameCount} games.",
                id, result.WeekNumber, result.GamesSimulated);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error advancing week for season {SeasonId}", id);
            return StatusCode(500, new { error = "Failed to advance week" });
        }
    }

    /// <summary>
    /// Reverts the last completed week, resetting game results and moving the season pointer back.
    /// </summary>
    /// <param name="id">Season ID.</param>
    /// <returns>Result of the revert operation.</returns>
    [HttpPost("{id}/revert-week")]
    [ProducesResponseType(typeof(SeasonSimulationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SeasonSimulationResult>> RevertWeek(int id)
    {
        try
        {
            var season = await _seasonRepository.GetByIdAsync(id);
            if (season == null)
            {
                return NotFound(new { error = $"Season with ID {id} not found" });
            }

            // Check authorization - must be Commissioner
            var azureAdObjectId = HttpContext.GetAzureAdObjectId();
            if (string.IsNullOrEmpty(azureAdObjectId))
            {
                return Unauthorized(new { error = "User identity not found in token" });
            }

            var isCommissioner = await _authorizationService.IsCommissionerOfLeagueAsync(azureAdObjectId, season.LeagueId);
            var isGlobalAdmin = await _authorizationService.IsGlobalAdminAsync(azureAdObjectId);

            if (!isCommissioner && !isGlobalAdmin)
            {
                return Forbid();
            }

            var result = await _seasonSimulationService.RevertLastWeekAsync(id);

            if (!result.Success)
            {
                return BadRequest(new { error = result.Error });
            }

            _logger.LogInformation(
                "Reverted season {SeasonId} to week {WeekNumber}.",
                id, result.WeekNumber);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reverting week for season {SeasonId}", id);
            return StatusCode(500, new { error = "Failed to revert week" });
        }
    }

    #region Mapping Methods

    private static SeasonDto MapToSeasonDto(Season season)
    {
        return new SeasonDto
        {
            Id = season.Id,
            LeagueId = season.LeagueId,
            Year = season.Year,
            CurrentWeek = season.CurrentWeek,
            Phase = season.Phase.ToString(),
            IsComplete = season.IsComplete,
            StartDate = season.StartDate,
            EndDate = season.EndDate,
            RegularSeasonWeeks = season.RegularSeasonWeeks,
            ChampionTeamId = season.ChampionTeamId,
            WeekCount = season.Weeks?.Count ?? 0,
            TotalGames = season.Weeks?.Sum(w => w.Games?.Count ?? 0) ?? 0
        };
    }

    private static SeasonDetailDto MapToSeasonDetailDto(Season season)
    {
        return new SeasonDetailDto
        {
            Id = season.Id,
            LeagueId = season.LeagueId,
            Year = season.Year,
            CurrentWeek = season.CurrentWeek,
            Phase = season.Phase.ToString(),
            IsComplete = season.IsComplete,
            StartDate = season.StartDate,
            EndDate = season.EndDate,
            RegularSeasonWeeks = season.RegularSeasonWeeks,
            ChampionTeamId = season.ChampionTeamId,
            WeekCount = season.Weeks?.Count ?? 0,
            TotalGames = season.Weeks?.Sum(w => w.Games?.Count ?? 0) ?? 0,
            Weeks = season.Weeks?
                .OrderBy(w => w.WeekNumber)
                .Select(MapToSeasonWeekDto)
                .ToList() ?? new List<SeasonWeekDto>()
        };
    }

    private static SeasonWeekDto MapToSeasonWeekDto(SeasonWeek week)
    {
        return new SeasonWeekDto
        {
            Id = week.Id,
            SeasonId = week.SeasonId,
            WeekNumber = week.WeekNumber,
            Phase = week.Phase.ToString(),
            Status = week.Status.ToString(),
            DisplayName = week.DisplayName,
            StartDate = week.StartDate,
            CompletedDate = week.CompletedDate,
            IsComplete = week.IsComplete,
            GameCount = week.Games?.Count ?? 0,
            Games = week.Games?
                .Select(MapToScheduledGameDto)
                .ToList() ?? new List<ScheduledGameDto>()
        };
    }

    private static ScheduledGameDto MapToScheduledGameDto(Game game)
    {
        return new ScheduledGameDto
        {
            Id = game.Id,
            HomeTeamId = game.HomeTeamId,
            HomeTeamName = game.HomeTeam?.Name ?? string.Empty,
            HomeTeamCity = game.HomeTeam?.City,
            AwayTeamId = game.AwayTeamId,
            AwayTeamName = game.AwayTeam?.Name ?? string.Empty,
            AwayTeamCity = game.AwayTeam?.City,
            HomeScore = game.HomeScore,
            AwayScore = game.AwayScore,
            IsComplete = game.IsComplete,
            PlayedAt = game.PlayedAt
        };
    }

    #endregion
}

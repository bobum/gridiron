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
/// Controller for league management operations (creation, structure management)
/// DOES NOT access the database directly - uses repositories from DataAccessLayer
/// REQUIRES AUTHENTICATION: All endpoints require valid Azure AD JWT token.
/// </summary>
[ApiController]
[Route("api/leagues-management")]
[Authorize] // All endpoints require authentication
public class LeaguesManagementController : ControllerBase
{
    private readonly ILeagueRepository _leagueRepository;
    private readonly ILeagueBuilderService _leagueBuilderService;
    private readonly IGridironAuthorizationService _authorizationService;
    private readonly ILogger<LeaguesManagementController> _logger;

    public LeaguesManagementController(
        ILeagueRepository leagueRepository,
        ILeagueBuilderService leagueBuilderService,
        IGridironAuthorizationService authorizationService,
        ILogger<LeaguesManagementController> logger)
    {
        _leagueRepository = leagueRepository ?? throw new ArgumentNullException(nameof(leagueRepository));
        _leagueBuilderService = leagueBuilderService ?? throw new ArgumentNullException(nameof(leagueBuilderService));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new league with specified structure.
    /// </summary>
    /// <param name="request">League creation request.</param>
    /// <returns>Created league with full structure.</returns>
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
            // Get current user from JWT claims
            var azureAdObjectId = HttpContext.GetAzureAdObjectId();
            var email = HttpContext.GetUserEmail();
            var displayName = HttpContext.GetUserDisplayName();

            if (string.IsNullOrEmpty(azureAdObjectId))
            {
                return Unauthorized(new { error = "User identity not found in token" });
            }

            // Sync user to database (create or update)
            var user = await _authorizationService.GetOrCreateUserFromClaimsAsync(
                azureAdObjectId,
                email ?? "unknown@example.com",
                displayName ?? "Unknown User");

            // Create league using LeagueBuilderService
            var league = _leagueBuilderService.CreateLeague(
                request.Name,
                request.NumberOfConferences,
                request.DivisionsPerConference,
                request.TeamsPerDivision);

            // Persist to database
            await _leagueRepository.AddAsync(league);

            // Auto-assign creator as Commissioner of the league
            user.LeagueRoles.Add(new DomainObjects.UserLeagueRole
            {
                UserId = user.Id,
                LeagueId = league.Id,
                Role = DomainObjects.UserRole.Commissioner,
                TeamId = null,
                AssignedAt = DateTime.UtcNow
            });
            await _leagueRepository.SaveChangesAsync();

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
                "Created league: {Name} (ID: {Id}) with {Conferences} conferences and {Teams} total teams. User {UserId} assigned as Commissioner.",
                league.Name, league.Id, leagueDetailDto.TotalConferences, leagueDetailDto.TotalTeams, user.Id);

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
    /// Gets a specific league by ID with full structure.
    /// </summary>
    /// <param name="id">League ID.</param>
    /// <returns>League with full structure.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(LeagueDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<LeagueDetailDto>> GetLeague(int id)
    {
        try
        {
            var league = await _leagueRepository.GetByIdWithFullStructureAsync(id);

            if (league == null)
            {
                return NotFound(new { error = $"League with ID {id} not found" });
            }

            // Check authorization: User must have access to this league
            var azureAdObjectId = HttpContext.GetAzureAdObjectId();
            if (string.IsNullOrEmpty(azureAdObjectId))
            {
                return Unauthorized(new { error = "User identity not found in token" });
            }

            var hasAccess = await _authorizationService.CanAccessLeagueAsync(azureAdObjectId, id);
            if (!hasAccess)
            {
                return Forbid();
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
    /// Gets all leagues (without full structure) that the current user has access to.
    /// </summary>
    /// <returns>List of accessible leagues.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<LeagueDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<LeagueDto>>> GetAllLeagues()
    {
        try
        {
            var azureAdObjectId = HttpContext.GetAzureAdObjectId();
            if (string.IsNullOrEmpty(azureAdObjectId))
            {
                return Unauthorized(new { error = "User identity not found in token" });
            }

            // Check if user is global admin
            var isGlobalAdmin = await _authorizationService.IsGlobalAdminAsync(azureAdObjectId);
            var leagues = await _leagueRepository.GetAllAsync();

            List<League> filteredLeagues;
            if (isGlobalAdmin)
            {
                // Global admins see all leagues
                filteredLeagues = leagues;
            }
            else
            {
                // Regular users only see leagues they have access to
                var accessibleLeagueIds = await _authorizationService.GetAccessibleLeagueIdsAsync(azureAdObjectId);
                filteredLeagues = leagues.Where(l => accessibleLeagueIds.Contains(l.Id)).ToList();
            }

            var leagueDtos = filteredLeagues.Select(l => new LeagueDto
            {
                Id = l.Id,
                Name = l.Name,
                Season = l.Season,
                IsActive = l.IsActive,
                TotalConferences = 0,  // Not loaded for list view
                TotalTeams = 0 // Not loaded for list view
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
    /// Populates rosters for all teams in a league with 53 randomly generated players each.
    /// </summary>
    /// <param name="id">League ID.</param>
    /// <param name="seed">Optional seed for reproducible generation.</param>
    /// <returns>Updated league with all teams populated.</returns>
    [HttpPost("{id}/populate-rosters")]
    [ProducesResponseType(typeof(LeagueDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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

            // Check authorization: User must be Commissioner of this league or Global Admin
            var azureAdObjectId = HttpContext.GetAzureAdObjectId();
            if (string.IsNullOrEmpty(azureAdObjectId))
            {
                return Unauthorized(new { error = "User identity not found in token" });
            }

            var isCommissioner = await _authorizationService.IsCommissionerOfLeagueAsync(azureAdObjectId, id);
            var isGlobalAdmin = await _authorizationService.IsGlobalAdminAsync(azureAdObjectId);

            if (!isCommissioner && !isGlobalAdmin)
            {
                return Forbid();
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

    /// <summary>
    /// Updates an existing league.
    /// </summary>
    /// <param name="id">League ID.</param>
    /// <param name="request">Update request with optional fields.</param>
    /// <returns>Updated league.</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(LeagueDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<LeagueDto>> UpdateLeague(int id, [FromBody] UpdateLeagueRequest request)
    {
        if (request == null)
        {
            return BadRequest(new { error = "Request body is required" });
        }

        try
        {
            // Get league from repository (excludes soft-deleted by default)
            var league = await _leagueRepository.GetByIdAsync(id);
            if (league == null)
            {
                return NotFound(new { error = $"League with ID {id} not found" });
            }

            // Check authorization: User must be Commissioner of this league or Global Admin
            var azureAdObjectId = HttpContext.GetAzureAdObjectId();
            if (string.IsNullOrEmpty(azureAdObjectId))
            {
                return Unauthorized(new { error = "User identity not found in token" });
            }

            var isCommissioner = await _authorizationService.IsCommissionerOfLeagueAsync(azureAdObjectId, id);
            var isGlobalAdmin = await _authorizationService.IsGlobalAdminAsync(azureAdObjectId);

            if (!isCommissioner && !isGlobalAdmin)
            {
                return Forbid();
            }

            // Use builder service to update league (domain logic)
            _leagueBuilderService.UpdateLeague(league, request.Name, request.Season, request.IsActive);

            // Persist changes
            await _leagueRepository.UpdateAsync(league);

            // Map to DTO and return
            var leagueDto = new LeagueDto
            {
                Id = league.Id,
                Name = league.Name,
                Season = league.Season,
                IsActive = league.IsActive,
                TotalConferences = 0,  // Not loaded for update response
                TotalTeams = 0 // Not loaded for update response
            };

            _logger.LogInformation(
                "Updated league {LeagueId}: Name={Name}, Season={Season}, IsActive={IsActive}",
                league.Id, league.Name, league.Season, league.IsActive);

            return Ok(leagueDto);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid update request for league {LeagueId}", id);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating league {LeagueId}", id);
            return StatusCode(500, new { error = "Failed to update league" });
        }
    }

    /// <summary>
    /// Soft deletes a league with cascade to all child entities.
    /// </summary>
    /// <param name="id">League ID.</param>
    /// <param name="deletedBy">Who is deleting the league.</param>
    /// <param name="reason">Reason for deletion.</param>
    /// <returns>Cascade delete result with statistics.</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(DomainObjects.CascadeDeleteResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<DomainObjects.CascadeDeleteResult>> DeleteLeague(
        int id,
        [FromQuery] string? deletedBy = null,
        [FromQuery] string? reason = null)
    {
        try
        {
            // Check authorization: User must be Commissioner of this league or Global Admin
            var azureAdObjectId = HttpContext.GetAzureAdObjectId();
            if (string.IsNullOrEmpty(azureAdObjectId))
            {
                return Unauthorized(new { error = "User identity not found in token" });
            }

            var isCommissioner = await _authorizationService.IsCommissionerOfLeagueAsync(azureAdObjectId, id);
            var isGlobalAdmin = await _authorizationService.IsGlobalAdminAsync(azureAdObjectId);

            if (!isCommissioner && !isGlobalAdmin)
            {
                return Forbid();
            }

            var result = await _leagueRepository.SoftDeleteWithCascadeAsync(id, deletedBy, reason);

            if (!result.Success)
            {
                return BadRequest(new { error = result.ErrorMessage, result });
            }

            _logger.LogInformation(
                "Cascade soft-deleted league {LeagueId}: {TotalDeleted} entities deleted ({Details})",
                id, result.TotalEntitiesDeleted, string.Join(", ", result.DeletedByType.Select(kvp => $"{kvp.Value} {kvp.Key}")));

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cascade soft-deleting league {LeagueId}", id);
            return StatusCode(500, new { error = "Failed to cascade soft-delete league" });
        }
    }

    /// <summary>
    /// Restores a soft-deleted league with optional cascade to child entities.
    /// </summary>
    /// <param name="id">League ID.</param>
    /// <param name="cascade">Whether to cascade restore to all child entities.</param>
    /// <returns>Cascade restore result with statistics.</returns>
    [HttpPost("{id}/restore")]
    [ProducesResponseType(typeof(DomainObjects.CascadeRestoreResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DomainObjects.CascadeRestoreResult>> RestoreLeague(
        int id,
        [FromQuery] bool cascade = false)
    {
        try
        {
            var result = await _leagueRepository.RestoreWithCascadeAsync(id, cascade);

            if (!result.Success)
            {
                return BadRequest(new { error = result.ErrorMessage, result });
            }

            _logger.LogInformation(
                "Cascade restored league {LeagueId}: {TotalRestored} entities restored ({Details})",
                id, result.TotalEntitiesRestored, string.Join(", ", result.RestoredByType.Select(kvp => $"{kvp.Value} {kvp.Key}")));

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cascade restoring league {LeagueId}", id);
            return StatusCode(500, new { error = "Failed to cascade restore league" });
        }
    }

    /// <summary>
    /// Validates whether a league can be restored.
    /// </summary>
    /// <param name="id">League ID.</param>
    /// <returns>Validation result.</returns>
    [HttpGet("{id}/validate-restore")]
    [ProducesResponseType(typeof(DomainObjects.RestoreValidationResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<DomainObjects.RestoreValidationResult>> ValidateRestore(int id)
    {
        try
        {
            var result = await _leagueRepository.ValidateRestoreAsync(id);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating restore for league {LeagueId}", id);
            return StatusCode(500, new { error = "Failed to validate restore" });
        }
    }

    /// <summary>
    /// Gets all soft-deleted leagues.
    /// </summary>
    /// <param name="season">Optional filter by season.</param>
    /// <returns>List of soft-deleted leagues.</returns>
    [HttpGet("deleted")]
    [ProducesResponseType(typeof(List<LeagueDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<LeagueDto>>> GetDeletedLeagues([FromQuery] int? season = null)
    {
        try
        {
            var deletedLeagues = await _leagueRepository.GetDeletedAsync();

            // Filter by season if provided
            if (season.HasValue)
            {
                deletedLeagues = deletedLeagues.Where(l => l.Season == season.Value).ToList();
            }

            var leagueDtos = deletedLeagues.Select(l => new LeagueDto
            {
                Id = l.Id,
                Name = l.Name,
                Season = l.Season,
                IsActive = l.IsActive,
                TotalConferences = 0,  // Not loaded for list view
                TotalTeams = 0 // Not loaded for list view
            }).ToList();

            return Ok(leagueDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving deleted leagues");
            return StatusCode(500, new { error = "Failed to retrieve deleted leagues" });
        }
    }
}

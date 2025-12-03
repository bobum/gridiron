using DataAccessLayer.Repositories;
using GameManagement.Services;
using Gridiron.WebApi.DTOs;
using Gridiron.WebApi.Extensions;
using Gridiron.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gridiron.WebApi.Controllers;

/// <summary>
/// Controller for division management operations
/// DOES NOT access the database directly - uses repositories from DataAccessLayer
/// REQUIRES AUTHENTICATION: All endpoints require valid Azure AD JWT token
/// AUTHORIZATION: Only Commissioners of the league can manage divisions.
/// </summary>
[ApiController]
[Route("api/divisions-management")]
[Authorize]
public class DivisionsManagementController : ControllerBase
{
    private readonly IDivisionRepository _divisionRepository;
    private readonly IConferenceRepository _conferenceRepository;
    private readonly IDivisionBuilderService _divisionBuilderService;
    private readonly IGridironAuthorizationService _authorizationService;
    private readonly ILogger<DivisionsManagementController> _logger;

    public DivisionsManagementController(
        IDivisionRepository divisionRepository,
        IConferenceRepository conferenceRepository,
        IDivisionBuilderService divisionBuilderService,
        IGridironAuthorizationService authorizationService,
        ILogger<DivisionsManagementController> logger)
    {
        _divisionRepository = divisionRepository ?? throw new ArgumentNullException(nameof(divisionRepository));
        _conferenceRepository = conferenceRepository ?? throw new ArgumentNullException(nameof(conferenceRepository));
        _divisionBuilderService = divisionBuilderService ?? throw new ArgumentNullException(nameof(divisionBuilderService));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a specific division by ID (cascades from league authorization via conference).
    /// </summary>
    /// <param name="id">Division ID.</param>
    /// <returns>Division details.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(DivisionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<DivisionDto>> GetDivision(int id)
    {
        try
        {
            // Get current user from JWT claims
            var azureAdObjectId = HttpContext.GetAzureAdObjectId();
            if (string.IsNullOrEmpty(azureAdObjectId))
            {
                return Unauthorized(new { error = "User identity not found in token" });
            }

            var division = await _divisionRepository.GetByIdWithTeamsAsync(id);

            if (division == null)
            {
                return NotFound(new { error = $"Division with ID {id} not found" });
            }

            // Get conference to determine league (Division -> Conference -> League)
            var conference = await _conferenceRepository.GetByIdAsync(division.ConferenceId);
            if (conference == null)
            {
                return NotFound(new { error = "Division's conference not found" });
            }

            // Check if user has access to the league
            var hasAccess = await _authorizationService.CanAccessLeagueAsync(azureAdObjectId, conference.LeagueId);
            if (!hasAccess)
            {
                return Forbid();
            }

            var divisionDto = new DivisionDto
            {
                Id = division.Id,
                Name = division.Name,
                Teams = division.Teams.Select(t => new TeamDto
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
            };

            return Ok(divisionDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving division {Id}", id);
            return StatusCode(500, new { error = "Failed to retrieve division" });
        }
    }

    /// <summary>
    /// Gets all divisions (filtered to divisions in leagues user has access to).
    /// </summary>
    /// <returns>List of accessible divisions.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<DivisionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<DivisionDto>>> GetAllDivisions()
    {
        try
        {
            // Get current user from JWT claims
            var azureAdObjectId = HttpContext.GetAzureAdObjectId();
            if (string.IsNullOrEmpty(azureAdObjectId))
            {
                return Unauthorized(new { error = "User identity not found in token" });
            }

            var divisions = await _divisionRepository.GetAllAsync();
            var conferences = await _conferenceRepository.GetAllAsync();

            // Get accessible league IDs
            var accessibleLeagueIds = await _authorizationService.GetAccessibleLeagueIdsAsync(azureAdObjectId);
            var isGlobalAdmin = await _authorizationService.IsGlobalAdminAsync(azureAdObjectId);

            // Filter divisions by checking their conference's league
            List<DomainObjects.Division> filteredDivisions;
            if (isGlobalAdmin)
            {
                filteredDivisions = divisions;
            }
            else
            {
                var conferenceDict = conferences.ToDictionary(c => c.Id, c => c.LeagueId);
                filteredDivisions = divisions.Where(d =>
                    conferenceDict.ContainsKey(d.ConferenceId) &&
                    accessibleLeagueIds.Contains(conferenceDict[d.ConferenceId]))
                .ToList();
            }

            var divisionDtos = filteredDivisions.Select(d => new DivisionDto
            {
                Id = d.Id,
                Name = d.Name,
                Teams = new List<TeamDto>() // Not loaded for list view
            }).ToList();

            return Ok(divisionDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all divisions");
            return StatusCode(500, new { error = "Failed to retrieve divisions" });
        }
    }

    /// <summary>
    /// Updates an existing division (Only Commissioners can update divisions in their league).
    /// </summary>
    /// <param name="id">Division ID.</param>
    /// <param name="request">Update request with optional fields.</param>
    /// <returns>Updated division.</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(DivisionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<DivisionDto>> UpdateDivision(int id, [FromBody] UpdateDivisionRequest request)
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

            // Get division from repository (excludes soft-deleted by default)
            var division = await _divisionRepository.GetByIdAsync(id);
            if (division == null)
            {
                return NotFound(new { error = $"Division with ID {id} not found" });
            }

            // Get conference to determine league
            var conference = await _conferenceRepository.GetByIdAsync(division.ConferenceId);
            if (conference == null)
            {
                return NotFound(new { error = "Division's conference not found" });
            }

            // Only Commissioners of the league can update divisions
            var isCommissioner = await _authorizationService.IsCommissionerOfLeagueAsync(azureAdObjectId, conference.LeagueId);
            var isGlobalAdmin = await _authorizationService.IsGlobalAdminAsync(azureAdObjectId);

            if (!isCommissioner && !isGlobalAdmin)
            {
                return Forbid();
            }

            // Use builder service to update division (domain logic)
            _divisionBuilderService.UpdateDivision(division, request.Name);

            // Persist changes
            await _divisionRepository.UpdateAsync(division);

            // Map to DTO and return
            var divisionDto = new DivisionDto
            {
                Id = division.Id,
                Name = division.Name,
                Teams = new List<TeamDto>() // Not loaded for update response
            };

            _logger.LogInformation(
                "Updated division {DivisionId}: Name={Name}",
                division.Id, division.Name);

            return Ok(divisionDto);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid update request for division {DivisionId}", id);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating division {DivisionId}", id);
            return StatusCode(500, new { error = "Failed to update division" });
        }
    }
}

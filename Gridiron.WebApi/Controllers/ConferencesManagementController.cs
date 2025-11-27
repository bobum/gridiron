using DataAccessLayer.Repositories;
using GameManagement.Services;
using Gridiron.WebApi.DTOs;
using Gridiron.WebApi.Extensions;
using Gridiron.WebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gridiron.WebApi.Controllers;

/// <summary>
/// Controller for conference management operations
/// DOES NOT access the database directly - uses repositories from DataAccessLayer
/// REQUIRES AUTHENTICATION: All endpoints require valid Azure AD JWT token
/// AUTHORIZATION: Only Commissioners of the league can manage conferences
/// </summary>
[ApiController]
[Route("api/conferences-management")]
[Authorize]
public class ConferencesManagementController : ControllerBase
{
    private readonly IConferenceRepository _conferenceRepository;
    private readonly IConferenceBuilderService _conferenceBuilderService;
    private readonly IGridironAuthorizationService _authorizationService;
    private readonly ILogger<ConferencesManagementController> _logger;

    public ConferencesManagementController(
        IConferenceRepository conferenceRepository,
        IConferenceBuilderService conferenceBuilderService,
        IGridironAuthorizationService authorizationService,
        ILogger<ConferencesManagementController> logger)
    {
        _conferenceRepository = conferenceRepository ?? throw new ArgumentNullException(nameof(conferenceRepository));
        _conferenceBuilderService = conferenceBuilderService ?? throw new ArgumentNullException(nameof(conferenceBuilderService));
        _authorizationService = authorizationService ?? throw new ArgumentNullException(nameof(authorizationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a specific conference by ID (cascades from league authorization)
    /// </summary>
    /// <param name="id">Conference ID</param>
    /// <returns>Conference details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ConferenceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ConferenceDto>> GetConference(int id)
    {
        try
        {
            // Get current user from JWT claims
            var azureAdObjectId = HttpContext.GetAzureAdObjectId();
            if (string.IsNullOrEmpty(azureAdObjectId))
            {
                return Unauthorized(new { error = "User identity not found in token" });
            }

            var conference = await _conferenceRepository.GetByIdWithDivisionsAsync(id);

            if (conference == null)
            {
                return NotFound(new { error = $"Conference with ID {id} not found" });
            }

            // Check if user has access to the league (conference access cascades from league)
            var hasAccess = await _authorizationService.CanAccessLeagueAsync(azureAdObjectId, conference.LeagueId);
            if (!hasAccess)
            {
                return Forbid();
            }

            var conferenceDto = new ConferenceDto
            {
                Id = conference.Id,
                Name = conference.Name,
                Divisions = conference.Divisions.Select(d => new DivisionDto
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
            };

            return Ok(conferenceDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving conference {Id}", id);
            return StatusCode(500, new { error = "Failed to retrieve conference" });
        }
    }

    /// <summary>
    /// Gets all conferences (filtered to only conferences in leagues user has access to)
    /// </summary>
    /// <returns>List of accessible conferences</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<ConferenceDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<ConferenceDto>>> GetAllConferences()
    {
        try
        {
            // Get current user from JWT claims
            var azureAdObjectId = HttpContext.GetAzureAdObjectId();
            if (string.IsNullOrEmpty(azureAdObjectId))
            {
                return Unauthorized(new { error = "User identity not found in token" });
            }

            var conferences = await _conferenceRepository.GetAllAsync();

            // Filter to conferences in leagues the user can access
            var accessibleLeagueIds = await _authorizationService.GetAccessibleLeagueIdsAsync(azureAdObjectId);
            var isGlobalAdmin = await _authorizationService.IsGlobalAdminAsync(azureAdObjectId);

            List<DomainObjects.Conference> filteredConferences;
            if (isGlobalAdmin)
            {
                filteredConferences = conferences;
            }
            else
            {
                filteredConferences = conferences.Where(c => accessibleLeagueIds.Contains(c.LeagueId)).ToList();
            }

            var conferenceDtos = filteredConferences.Select(c => new ConferenceDto
            {
                Id = c.Id,
                Name = c.Name,
                Divisions = new List<DivisionDto>()  // Not loaded for list view
            }).ToList();

            return Ok(conferenceDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all conferences");
            return StatusCode(500, new { error = "Failed to retrieve conferences" });
        }
    }

    /// <summary>
    /// Updates an existing conference (Only Commissioners can update conferences in their league)
    /// </summary>
    /// <param name="id">Conference ID</param>
    /// <param name="request">Update request with optional fields</param>
    /// <returns>Updated conference</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ConferenceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ConferenceDto>> UpdateConference(int id, [FromBody] UpdateConferenceRequest request)
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

            // Get conference from repository (excludes soft-deleted by default)
            var conference = await _conferenceRepository.GetByIdAsync(id);
            if (conference == null)
            {
                return NotFound(new { error = $"Conference with ID {id} not found" });
            }

            // Only Commissioners of the league can update conferences
            var isCommissioner = await _authorizationService.IsCommissionerOfLeagueAsync(azureAdObjectId, conference.LeagueId);
            var isGlobalAdmin = await _authorizationService.IsGlobalAdminAsync(azureAdObjectId);

            if (!isCommissioner && !isGlobalAdmin)
            {
                return Forbid();
            }

            // Use builder service to update conference (domain logic)
            _conferenceBuilderService.UpdateConference(conference, request.Name);

            // Persist changes
            await _conferenceRepository.UpdateAsync(conference);

            // Map to DTO and return
            var conferenceDto = new ConferenceDto
            {
                Id = conference.Id,
                Name = conference.Name,
                Divisions = new List<DivisionDto>()  // Not loaded for update response
            };

            _logger.LogInformation(
                "Updated conference {ConferenceId}: Name={Name}",
                conference.Id, conference.Name);

            return Ok(conferenceDto);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid update request for conference {ConferenceId}", id);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating conference {ConferenceId}", id);
            return StatusCode(500, new { error = "Failed to update conference" });
        }
    }
}

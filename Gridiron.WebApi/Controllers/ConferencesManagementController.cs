using DataAccessLayer.Repositories;
using GameManagement.Services;
using Gridiron.WebApi.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Gridiron.WebApi.Controllers;

/// <summary>
/// Controller for conference management operations
/// DOES NOT access the database directly - uses repositories from DataAccessLayer
/// </summary>
[ApiController]
[Route("api/conferences-management")]
public class ConferencesManagementController : ControllerBase
{
    private readonly IConferenceRepository _conferenceRepository;
    private readonly IConferenceBuilderService _conferenceBuilderService;
    private readonly ILogger<ConferencesManagementController> _logger;

    public ConferencesManagementController(
        IConferenceRepository conferenceRepository,
        IConferenceBuilderService conferenceBuilderService,
        ILogger<ConferencesManagementController> logger)
    {
        _conferenceRepository = conferenceRepository ?? throw new ArgumentNullException(nameof(conferenceRepository));
        _conferenceBuilderService = conferenceBuilderService ?? throw new ArgumentNullException(nameof(conferenceBuilderService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a specific conference by ID
    /// </summary>
    /// <param name="id">Conference ID</param>
    /// <returns>Conference details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ConferenceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConferenceDto>> GetConference(int id)
    {
        try
        {
            var conference = await _conferenceRepository.GetByIdWithDivisionsAsync(id);

            if (conference == null)
            {
                return NotFound(new { error = $"Conference with ID {id} not found" });
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
    /// Gets all conferences
    /// </summary>
    /// <returns>List of all conferences</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<ConferenceDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ConferenceDto>>> GetAllConferences()
    {
        try
        {
            var conferences = await _conferenceRepository.GetAllAsync();

            var conferenceDtos = conferences.Select(c => new ConferenceDto
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
    /// Updates an existing conference
    /// </summary>
    /// <param name="id">Conference ID</param>
    /// <param name="request">Update request with optional fields</param>
    /// <returns>Updated conference</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ConferenceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConferenceDto>> UpdateConference(int id, [FromBody] UpdateConferenceRequest request)
    {
        if (request == null)
        {
            return BadRequest(new { error = "Request body is required" });
        }

        try
        {
            // Get conference from repository (excludes soft-deleted by default)
            var conference = await _conferenceRepository.GetByIdAsync(id);
            if (conference == null)
            {
                return NotFound(new { error = $"Conference with ID {id} not found" });
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

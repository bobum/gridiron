using DataAccessLayer.Repositories;
using GameManagement.Services;
using Gridiron.WebApi.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Gridiron.WebApi.Controllers;

/// <summary>
/// Controller for division management operations
/// DOES NOT access the database directly - uses repositories from DataAccessLayer
/// </summary>
[ApiController]
[Route("api/divisions-management")]
public class DivisionsManagementController : ControllerBase
{
    private readonly IDivisionRepository _divisionRepository;
    private readonly IDivisionBuilderService _divisionBuilderService;
    private readonly ILogger<DivisionsManagementController> _logger;

    public DivisionsManagementController(
        IDivisionRepository divisionRepository,
        IDivisionBuilderService divisionBuilderService,
        ILogger<DivisionsManagementController> logger)
    {
        _divisionRepository = divisionRepository ?? throw new ArgumentNullException(nameof(divisionRepository));
        _divisionBuilderService = divisionBuilderService ?? throw new ArgumentNullException(nameof(divisionBuilderService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a specific division by ID
    /// </summary>
    /// <param name="id">Division ID</param>
    /// <returns>Division details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(DivisionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DivisionDto>> GetDivision(int id)
    {
        try
        {
            var division = await _divisionRepository.GetByIdWithTeamsAsync(id);

            if (division == null)
            {
                return NotFound(new { error = $"Division with ID {id} not found" });
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
    /// Gets all divisions
    /// </summary>
    /// <returns>List of all divisions</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<DivisionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<DivisionDto>>> GetAllDivisions()
    {
        try
        {
            var divisions = await _divisionRepository.GetAllAsync();

            var divisionDtos = divisions.Select(d => new DivisionDto
            {
                Id = d.Id,
                Name = d.Name,
                Teams = new List<TeamDto>()  // Not loaded for list view
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
    /// Updates an existing division
    /// </summary>
    /// <param name="id">Division ID</param>
    /// <param name="request">Update request with optional fields</param>
    /// <returns>Updated division</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(DivisionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DivisionDto>> UpdateDivision(int id, [FromBody] UpdateDivisionRequest request)
    {
        if (request == null)
        {
            return BadRequest(new { error = "Request body is required" });
        }

        try
        {
            // Get division from repository (excludes soft-deleted by default)
            var division = await _divisionRepository.GetByIdAsync(id);
            if (division == null)
            {
                return NotFound(new { error = $"Division with ID {id} not found" });
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
                Teams = new List<TeamDto>()  // Not loaded for update response
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

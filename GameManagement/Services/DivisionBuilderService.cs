using DomainObjects;
using Microsoft.Extensions.Logging;

namespace GameManagement.Services;

/// <summary>
/// Service for building and managing divisions.
/// </summary>
public class DivisionBuilderService : IDivisionBuilderService
{
    private readonly ILogger<DivisionBuilderService> _logger;

    public DivisionBuilderService(ILogger<DivisionBuilderService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void UpdateDivision(Division division, string? newName)
    {
        if (division == null)
        {
            throw new ArgumentNullException(nameof(division));
        }

        // Update name if provided and not empty
        if (!string.IsNullOrWhiteSpace(newName))
        {
            _logger.LogInformation(
                "Updating division {DivisionId} name from '{OldName}' to '{NewName}'",
                division.Id, division.Name, newName);

            division.Name = newName;
        }
    }
}

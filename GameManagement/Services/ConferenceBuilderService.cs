using DomainObjects;
using Microsoft.Extensions.Logging;

namespace GameManagement.Services;

/// <summary>
/// Service for building and managing conferences
/// </summary>
public class ConferenceBuilderService : IConferenceBuilderService
{
    private readonly ILogger<ConferenceBuilderService> _logger;

    public ConferenceBuilderService(ILogger<ConferenceBuilderService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void UpdateConference(Conference conference, string? newName)
    {
        if (conference == null)
            throw new ArgumentNullException(nameof(conference));

        // Update name if provided and not empty
        if (!string.IsNullOrWhiteSpace(newName))
        {
            _logger.LogInformation(
                "Updating conference {ConferenceId} name from '{OldName}' to '{NewName}'",
                conference.Id, conference.Name, newName);

            conference.Name = newName;
        }
    }
}

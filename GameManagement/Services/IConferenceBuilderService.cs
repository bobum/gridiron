using DomainObjects;

namespace GameManagement.Services;

/// <summary>
/// Service interface for building and managing conferences.
/// </summary>
public interface IConferenceBuilderService
{
    /// <summary>
    /// Updates a conference with new values.
    /// </summary>
    /// <param name="conference">The conference to update.</param>
    /// <param name="newName">Optional new name for the conference.</param>
    void UpdateConference(Conference conference, string? newName);
}

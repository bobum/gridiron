using DomainObjects;

namespace GameManagement.Services;

/// <summary>
/// Service interface for building and managing divisions.
/// </summary>
public interface IDivisionBuilderService
{
    /// <summary>
    /// Updates a division with new values.
    /// </summary>
    /// <param name="division">The division to update.</param>
    /// <param name="newName">Optional new name for the division.</param>
    void UpdateDivision(Division division, string? newName);
}

using DomainObjects;

namespace DataAccessLayer.Repositories;

/// <summary>
/// Repository interface for Division data access
/// This is the ONLY way to access division data from the database.
/// </summary>
public interface IDivisionRepository
{
    /// <summary>
    /// Gets all divisions.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<List<Division>> GetAllAsync();

    /// <summary>
    /// Gets a division by ID.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<Division?> GetByIdAsync(int divisionId);

    /// <summary>
    /// Gets a division by ID with teams.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<Division?> GetByIdWithTeamsAsync(int divisionId);

    /// <summary>
    /// Adds a new division.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<Division> AddAsync(Division division);

    /// <summary>
    /// Updates an existing division.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task UpdateAsync(Division division);

    /// <summary>
    /// Deletes a division.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task DeleteAsync(int divisionId);

    /// <summary>
    /// Soft deletes a division by marking it as deleted.
    /// </summary>
    /// <param name="divisionId">ID of the division to soft delete.</param>
    /// <param name="deletedBy">Username or identifier of who is deleting.</param>
    /// <param name="reason">Optional reason for deletion.</param>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task SoftDeleteAsync(int divisionId, string? deletedBy = null, string? reason = null);

    /// <summary>
    /// Restores a soft-deleted division.
    /// </summary>
    /// <param name="divisionId">ID of the division to restore.</param>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task RestoreAsync(int divisionId);

    /// <summary>
    /// Gets all soft-deleted divisions.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<List<Division>> GetDeletedAsync();

    /// <summary>
    /// Saves all pending changes.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<int> SaveChangesAsync();
}

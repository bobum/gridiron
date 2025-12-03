namespace DataAccessLayer.Repositories;

/// <summary>
/// Repository for player generation data (names, colleges)
/// Abstraction allows switching between database and JSON sources.
/// </summary>
public interface IPlayerDataRepository
{
    /// <summary>
    /// Gets list of first names for player generation.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<List<string>> GetFirstNamesAsync();

    /// <summary>
    /// Gets list of last names for player generation.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<List<string>> GetLastNamesAsync();

    /// <summary>
    /// Gets list of colleges for player generation.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<List<string>> GetCollegesAsync();
}

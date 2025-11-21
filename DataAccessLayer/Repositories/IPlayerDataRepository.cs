namespace DataAccessLayer.Repositories;

/// <summary>
/// Repository for player generation data (names, colleges)
/// Abstraction allows switching between database and JSON sources
/// </summary>
public interface IPlayerDataRepository
{
    /// <summary>
    /// Gets list of first names for player generation
    /// </summary>
    Task<List<string>> GetFirstNamesAsync();

    /// <summary>
    /// Gets list of last names for player generation
    /// </summary>
    Task<List<string>> GetLastNamesAsync();

    /// <summary>
    /// Gets list of colleges for player generation
    /// </summary>
    Task<List<string>> GetCollegesAsync();
}

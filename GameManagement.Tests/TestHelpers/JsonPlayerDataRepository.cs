using System.Text.Json;
using DataAccessLayer.Repositories;

namespace GameManagement.Tests.TestHelpers;

/// <summary>
/// Test implementation of IPlayerDataRepository
/// Loads player generation data from JSON files instead of database
/// </summary>
public class JsonPlayerDataRepository : IPlayerDataRepository
{
    private readonly string _firstNamesPath;
    private readonly string _lastNamesPath;
    private readonly string _collegesPath;

    /// <summary>
    /// Creates repository with paths to JSON files
    /// </summary>
    /// <param name="testDataDirectory">Base directory containing JSON files</param>
    public JsonPlayerDataRepository(string testDataDirectory)
    {
        if (string.IsNullOrEmpty(testDataDirectory))
            throw new ArgumentNullException(nameof(testDataDirectory));

        if (!Directory.Exists(testDataDirectory))
            throw new DirectoryNotFoundException($"Test data directory not found: {testDataDirectory}");

        _firstNamesPath = Path.Combine(testDataDirectory, "FirstNames.json");
        _lastNamesPath = Path.Combine(testDataDirectory, "LastNames.json");
        _collegesPath = Path.Combine(testDataDirectory, "Colleges.json");

        // Fail fast - verify all files exist at construction time
        if (!File.Exists(_firstNamesPath))
            throw new FileNotFoundException($"FirstNames.json not found at: {_firstNamesPath}");

        if (!File.Exists(_lastNamesPath))
            throw new FileNotFoundException($"LastNames.json not found at: {_lastNamesPath}");

        if (!File.Exists(_collegesPath))
            throw new FileNotFoundException($"Colleges.json not found at: {_collegesPath}");
    }

    /// <summary>
    /// Gets list of first names from JSON file
    /// </summary>
    public async Task<List<string>> GetFirstNamesAsync()
    {
        try
        {
            var json = await File.ReadAllTextAsync(_firstNamesPath);
            var names = JsonSerializer.Deserialize<List<string>>(json);

            if (names == null || names.Count == 0)
                throw new InvalidOperationException($"FirstNames.json is empty or invalid at: {_firstNamesPath}");

            return names;
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException($"Failed to load first names from: {_firstNamesPath}", ex);
        }
    }

    /// <summary>
    /// Gets list of last names from JSON file
    /// </summary>
    public async Task<List<string>> GetLastNamesAsync()
    {
        try
        {
            var json = await File.ReadAllTextAsync(_lastNamesPath);
            var names = JsonSerializer.Deserialize<List<string>>(json);

            if (names == null || names.Count == 0)
                throw new InvalidOperationException($"LastNames.json is empty or invalid at: {_lastNamesPath}");

            return names;
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException($"Failed to load last names from: {_lastNamesPath}", ex);
        }
    }

    /// <summary>
    /// Gets list of colleges from JSON file
    /// </summary>
    public async Task<List<string>> GetCollegesAsync()
    {
        try
        {
            var json = await File.ReadAllTextAsync(_collegesPath);
            var colleges = JsonSerializer.Deserialize<List<string>>(json);

            if (colleges == null || colleges.Count == 0)
                throw new InvalidOperationException($"Colleges.json is empty or invalid at: {_collegesPath}");

            return colleges;
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException($"Failed to load colleges from: {_collegesPath}", ex);
        }
    }
}

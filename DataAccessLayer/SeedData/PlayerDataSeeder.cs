using System.Text.Json;
using DomainObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataAccessLayer.SeedData;

/// <summary>
/// Seeds FirstNames, LastNames, and Colleges tables from JSON files
/// Can be used during database initialization or for re-seeding data.
/// </summary>
public class PlayerDataSeeder
{
    private readonly GridironDbContext _context;
    private readonly ILogger<PlayerDataSeeder> _logger;

    public PlayerDataSeeder(GridironDbContext context, ILogger<PlayerDataSeeder> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Seeds all player generation data from JSON files.
    /// </summary>
    /// <param name="dataDirectory">Directory containing FirstNames.json, LastNames.json, and Colleges.json.</param>
    /// <param name="clearExisting">If true, deletes existing data before seeding.</param>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    public async Task SeedAllAsync(string dataDirectory, bool clearExisting = false)
    {
        if (string.IsNullOrEmpty(dataDirectory))
        {
            throw new ArgumentNullException(nameof(dataDirectory));
        }

        if (!Directory.Exists(dataDirectory))
        {
            throw new DirectoryNotFoundException($"Data directory not found: {dataDirectory}");
        }

        _logger.LogInformation("Starting player data seeding from: {DataDirectory}", dataDirectory);

        try
        {
            await SeedFirstNamesAsync(Path.Combine(dataDirectory, "FirstNames.json"), clearExisting);
            await SeedLastNamesAsync(Path.Combine(dataDirectory, "LastNames.json"), clearExisting);
            await SeedCollegesAsync(Path.Combine(dataDirectory, "Colleges.json"), clearExisting);

            _logger.LogInformation("Player data seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to seed player data");
            throw;
        }
    }

    /// <summary>
    /// Seeds FirstNames table from JSON file.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    public async Task SeedFirstNamesAsync(string jsonFilePath, bool clearExisting = false)
    {
        if (!File.Exists(jsonFilePath))
        {
            throw new FileNotFoundException($"FirstNames.json not found at: {jsonFilePath}");
        }

        _logger.LogInformation("Seeding FirstNames from: {FilePath}", jsonFilePath);

        if (clearExisting)
        {
            _logger.LogWarning("Clearing existing FirstNames data");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM FirstNames");
        }

        // Check if already seeded
        var existingCount = await _context.FirstNames.CountAsync();
        if (existingCount > 0)
        {
            _logger.LogInformation("FirstNames table already contains {Count} records, skipping seed", existingCount);
            return;
        }

        var json = await File.ReadAllTextAsync(jsonFilePath);
        var names = JsonSerializer.Deserialize<List<string>>(json);

        if (names == null || names.Count == 0)
        {
            throw new InvalidOperationException($"No names found in {jsonFilePath}");
        }

        var entities = names.Select(name => new FirstName { Name = name }).ToList();
        await _context.FirstNames.AddRangeAsync(entities);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} first names", names.Count);
    }

    /// <summary>
    /// Seeds LastNames table from JSON file.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    public async Task SeedLastNamesAsync(string jsonFilePath, bool clearExisting = false)
    {
        if (!File.Exists(jsonFilePath))
        {
            throw new FileNotFoundException($"LastNames.json not found at: {jsonFilePath}");
        }

        _logger.LogInformation("Seeding LastNames from: {FilePath}", jsonFilePath);

        if (clearExisting)
        {
            _logger.LogWarning("Clearing existing LastNames data");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM LastNames");
        }

        // Check if already seeded
        var existingCount = await _context.LastNames.CountAsync();
        if (existingCount > 0)
        {
            _logger.LogInformation("LastNames table already contains {Count} records, skipping seed", existingCount);
            return;
        }

        var json = await File.ReadAllTextAsync(jsonFilePath);
        var names = JsonSerializer.Deserialize<List<string>>(json);

        if (names == null || names.Count == 0)
        {
            throw new InvalidOperationException($"No names found in {jsonFilePath}");
        }

        var entities = names.Select(name => new LastName { Name = name }).ToList();
        await _context.LastNames.AddRangeAsync(entities);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} last names", names.Count);
    }

    /// <summary>
    /// Seeds Colleges table from JSON file.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    public async Task SeedCollegesAsync(string jsonFilePath, bool clearExisting = false)
    {
        if (!File.Exists(jsonFilePath))
        {
            throw new FileNotFoundException($"Colleges.json not found at: {jsonFilePath}");
        }

        _logger.LogInformation("Seeding Colleges from: {FilePath}", jsonFilePath);

        if (clearExisting)
        {
            _logger.LogWarning("Clearing existing Colleges data");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM Colleges");
        }

        // Check if already seeded
        var existingCount = await _context.Colleges.CountAsync();
        if (existingCount > 0)
        {
            _logger.LogInformation("Colleges table already contains {Count} records, skipping seed", existingCount);
            return;
        }

        var json = await File.ReadAllTextAsync(jsonFilePath);
        var colleges = JsonSerializer.Deserialize<List<string>>(json);

        if (colleges == null || colleges.Count == 0)
        {
            throw new InvalidOperationException($"No colleges found in {jsonFilePath}");
        }

        var entities = colleges.Select(college => new College { Name = college }).ToList();
        await _context.Colleges.AddRangeAsync(entities);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Seeded {Count} colleges", colleges.Count);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataAccessLayer.Repositories;

/// <summary>
/// Production implementation of IPlayerDataRepository
/// Queries FirstNames, LastNames, and Colleges tables from Azure SQL database
/// </summary>
public class DatabasePlayerDataRepository : IPlayerDataRepository
{
    private readonly GridironDbContext _context;
    private readonly ILogger<DatabasePlayerDataRepository> _logger;

    public DatabasePlayerDataRepository(
        GridironDbContext context,
        ILogger<DatabasePlayerDataRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets list of first names from database
    /// </summary>
    public async Task<List<string>> GetFirstNamesAsync()
    {
        try
        {
            _logger.LogDebug("Fetching first names from database");
            var names = await _context.FirstNames
                .Select(f => f.Name)
                .ToListAsync();

            if (names.Count == 0)
            {
                var error = "No first names found in database. Database may not be seeded.";
                _logger.LogError(error);
                throw new InvalidOperationException(error);
            }

            _logger.LogDebug("Retrieved {Count} first names from database", names.Count);
            return names;
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            _logger.LogError(ex, "Failed to retrieve first names from database");
            throw;
        }
    }

    /// <summary>
    /// Gets list of last names from database
    /// </summary>
    public async Task<List<string>> GetLastNamesAsync()
    {
        try
        {
            _logger.LogDebug("Fetching last names from database");
            var names = await _context.LastNames
                .Select(l => l.Name)
                .ToListAsync();

            if (names.Count == 0)
            {
                var error = "No last names found in database. Database may not be seeded.";
                _logger.LogError(error);
                throw new InvalidOperationException(error);
            }

            _logger.LogDebug("Retrieved {Count} last names from database", names.Count);
            return names;
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            _logger.LogError(ex, "Failed to retrieve last names from database");
            throw;
        }
    }

    /// <summary>
    /// Gets list of colleges from database
    /// </summary>
    public async Task<List<string>> GetCollegesAsync()
    {
        try
        {
            _logger.LogDebug("Fetching colleges from database");
            var colleges = await _context.Colleges
                .Select(c => c.Name)
                .ToListAsync();

            if (colleges.Count == 0)
            {
                var error = "No colleges found in database. Database may not be seeded.";
                _logger.LogError(error);
                throw new InvalidOperationException(error);
            }

            _logger.LogDebug("Retrieved {Count} colleges from database", colleges.Count);
            return colleges;
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            _logger.LogError(ex, "Failed to retrieve colleges from database");
            throw;
        }
    }
}

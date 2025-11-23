using DomainObjects;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories;

/// <summary>
/// Repository implementation for League data access
/// ALL database access for leagues goes through this class
/// </summary>
public class LeagueRepository : ILeagueRepository
{
    private readonly GridironDbContext _context;

    public LeagueRepository(GridironDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<List<League>> GetAllAsync()
    {
        return await _context.Leagues.ToListAsync();
    }

    public async Task<League?> GetByIdAsync(int leagueId)
    {
        return await _context.Leagues.FirstOrDefaultAsync(l => l.Id == leagueId);
    }

    public async Task<League?> GetByIdWithFullStructureAsync(int leagueId)
    {
        return await _context.Leagues
            .Include(l => l.Conferences)
                .ThenInclude(c => c.Divisions)
                    .ThenInclude(d => d.Teams)
            .FirstOrDefaultAsync(l => l.Id == leagueId);
    }

    public async Task<League?> GetByNameAsync(string name)
    {
        return await _context.Leagues
            .FirstOrDefaultAsync(l => l.Name == name);
    }

    public async Task<League> AddAsync(League league)
    {
        await _context.Leagues.AddAsync(league);
        await _context.SaveChangesAsync();
        return league;
    }

    public async Task UpdateAsync(League league)
    {
        _context.Leagues.Update(league);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int leagueId)
    {
        var league = await _context.Leagues.FindAsync(leagueId);
        if (league != null)
        {
            _context.Leagues.Remove(league);
            await _context.SaveChangesAsync();
        }
    }

    public async Task SoftDeleteAsync(int leagueId, string? deletedBy = null, string? reason = null)
    {
        var league = await _context.Leagues
            .IgnoreQueryFilters()  // Include soft-deleted entities
            .FirstOrDefaultAsync(l => l.Id == leagueId);

        if (league == null)
            throw new InvalidOperationException($"League with ID {leagueId} not found");

        if (league.IsDeleted)
            throw new InvalidOperationException($"League with ID {leagueId} is already deleted");

        league.SoftDelete(deletedBy, reason);
        await _context.SaveChangesAsync();
    }

    public async Task RestoreAsync(int leagueId)
    {
        var league = await _context.Leagues
            .IgnoreQueryFilters()  // Include soft-deleted entities
            .FirstOrDefaultAsync(l => l.Id == leagueId);

        if (league == null)
            throw new InvalidOperationException($"League with ID {leagueId} not found");

        if (!league.IsDeleted)
            throw new InvalidOperationException($"League with ID {leagueId} is not deleted");

        league.Restore();
        await _context.SaveChangesAsync();
    }

    public async Task<List<League>> GetDeletedAsync()
    {
        return await _context.Leagues
            .IgnoreQueryFilters()  // Include soft-deleted entities
            .Where(l => l.IsDeleted)
            .ToListAsync();
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}

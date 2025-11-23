using DomainObjects;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories;

/// <summary>
/// Repository implementation for PlayByPlay data access
/// ALL database access for play-by-play data goes through this class
/// </summary>
public class PlayByPlayRepository : IPlayByPlayRepository
{
    private readonly GridironDbContext _context;

    public PlayByPlayRepository(GridironDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<PlayByPlay?> GetByGameIdAsync(int gameId)
    {
        return await _context.PlayByPlays
            .Include(p => p.Game)
            .FirstOrDefaultAsync(p => p.GameId == gameId);
    }

    public async Task<PlayByPlay?> GetByIdAsync(int id)
    {
        return await _context.PlayByPlays
            .Include(p => p.Game)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<PlayByPlay> AddAsync(PlayByPlay playByPlay)
    {
        await _context.PlayByPlays.AddAsync(playByPlay);
        await _context.SaveChangesAsync();
        return playByPlay;
    }

    public async Task UpdateAsync(PlayByPlay playByPlay)
    {
        _context.PlayByPlays.Update(playByPlay);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var playByPlay = await _context.PlayByPlays.FindAsync(id);
        if (playByPlay != null)
        {
            _context.PlayByPlays.Remove(playByPlay);
            await _context.SaveChangesAsync();
        }
    }

    public async Task SoftDeleteAsync(int id, string? deletedBy = null, string? reason = null)
    {
        var playByPlay = await _context.PlayByPlays
            .IgnoreQueryFilters()  // Include soft-deleted entities
            .FirstOrDefaultAsync(p => p.Id == id);

        if (playByPlay == null)
            throw new InvalidOperationException($"PlayByPlay with ID {id} not found");

        if (playByPlay.IsDeleted)
            throw new InvalidOperationException($"PlayByPlay with ID {id} is already deleted");

        playByPlay.SoftDelete(deletedBy, reason);
        await _context.SaveChangesAsync();
    }

    public async Task RestoreAsync(int id)
    {
        var playByPlay = await _context.PlayByPlays
            .IgnoreQueryFilters()  // Include soft-deleted entities
            .FirstOrDefaultAsync(p => p.Id == id);

        if (playByPlay == null)
            throw new InvalidOperationException($"PlayByPlay with ID {id} not found");

        if (!playByPlay.IsDeleted)
            throw new InvalidOperationException($"PlayByPlay with ID {id} is not deleted");

        playByPlay.Restore();
        await _context.SaveChangesAsync();
    }

    public async Task<List<PlayByPlay>> GetDeletedAsync()
    {
        return await _context.PlayByPlays
            .IgnoreQueryFilters()  // Include soft-deleted entities
            .Where(p => p.IsDeleted)
            .OrderByDescending(p => p.DeletedAt)
            .ToListAsync();
    }

    public async Task<bool> ExistsForGameAsync(int gameId)
    {
        return await _context.PlayByPlays.AnyAsync(p => p.GameId == gameId);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}

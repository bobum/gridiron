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

    public async Task<bool> ExistsForGameAsync(int gameId)
    {
        return await _context.PlayByPlays.AnyAsync(p => p.GameId == gameId);
    }
}

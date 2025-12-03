using DomainObjects;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories;

/// <summary>
/// Repository implementation for Player data access
/// ALL database access for players goes through this class.
/// </summary>
public class PlayerRepository : IPlayerRepository
{
    private readonly GridironDbContext _context;

    public PlayerRepository(GridironDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<List<Player>> GetAllAsync()
    {
        return await _context.Players.ToListAsync();
    }

    public async Task<List<Player>> GetByTeamIdAsync(int teamId)
    {
        return await _context.Players
            .Where(p => p.TeamId == teamId)
            .ToListAsync();
    }

    public async Task<Player?> GetByIdAsync(int playerId)
    {
        return await _context.Players.FindAsync(playerId);
    }

    public async Task<Player> AddAsync(Player player)
    {
        await _context.Players.AddAsync(player);
        await _context.SaveChangesAsync();
        return player;
    }

    public async Task UpdateAsync(Player player)
    {
        _context.Players.Update(player);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int playerId)
    {
        var player = await _context.Players.FindAsync(playerId);
        if (player != null)
        {
            _context.Players.Remove(player);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}

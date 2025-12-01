using DomainObjects;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories;

/// <summary>
/// Repository implementation for Game data access
/// ALL database access for games goes through this class
/// </summary>
public class GameRepository : IGameRepository
{
    private readonly GridironDbContext _context;

    public GameRepository(GridironDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<List<Game>> GetAllAsync()
    {
        return await _context.Games
            .Include(g => g.HomeTeam)
            .Include(g => g.AwayTeam)
            .OrderByDescending(g => g.Id)
            .ToListAsync();
    }

    public async Task<Game?> GetByIdAsync(int gameId)
    {
        return await _context.Games.FirstOrDefaultAsync(g => g.Id == gameId);
    }

    public async Task<Game?> GetByIdWithTeamsAsync(int gameId)
    {
        return await _context.Games
            .Include(g => g.HomeTeam)
            .Include(g => g.AwayTeam)
            .FirstOrDefaultAsync(g => g.Id == gameId);
    }

    public async Task<Game?> GetByIdWithPlayByPlayAsync(int gameId)
    {
        return await _context.Games
            .Include(g => g.HomeTeam)
            .Include(g => g.AwayTeam)
            .Include(g => g.PlayByPlay)
            .FirstOrDefaultAsync(g => g.Id == gameId);
    }

    public async Task<Game> AddAsync(Game game)
    {
        await _context.Games.AddAsync(game);
        await _context.SaveChangesAsync();
        return game;
    }

    public async Task UpdateAsync(Game game)
    {
        _context.Games.Update(game);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int gameId)
    {
        var game = await _context.Games.FindAsync(gameId);
        if (game != null)
        {
            _context.Games.Remove(game);
            await _context.SaveChangesAsync();
        }
    }

    public async Task SoftDeleteAsync(int gameId, string? deletedBy = null, string? reason = null)
    {
        var game = await _context.Games
            .IgnoreQueryFilters()  // Include soft-deleted entities
            .FirstOrDefaultAsync(g => g.Id == gameId);

        if (game == null)
            throw new InvalidOperationException($"Game with ID {gameId} not found");

        if (game.IsDeleted)
            throw new InvalidOperationException($"Game with ID {gameId} is already deleted");

        game.SoftDelete(deletedBy, reason);
        await _context.SaveChangesAsync();
    }

    public async Task RestoreAsync(int gameId)
    {
        var game = await _context.Games
            .IgnoreQueryFilters()  // Include soft-deleted entities
            .FirstOrDefaultAsync(g => g.Id == gameId);

        if (game == null)
            throw new InvalidOperationException($"Game with ID {gameId} not found");

        if (!game.IsDeleted)
            throw new InvalidOperationException($"Game with ID {gameId} is not deleted");

        game.Restore();
        await _context.SaveChangesAsync();
    }

    public async Task<List<Game>> GetDeletedAsync()
    {
        return await _context.Games
            .IgnoreQueryFilters()  // Include soft-deleted entities
            .Where(g => g.IsDeleted)
            .OrderByDescending(g => g.DeletedAt)
            .ToListAsync();
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}

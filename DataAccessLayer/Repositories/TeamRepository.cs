using DomainObjects;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories;

/// <summary>
/// Repository implementation for Team data access
/// ALL database access for teams goes through this class
/// </summary>
public class TeamRepository : ITeamRepository
{
    private readonly GridironDbContext _context;

    public TeamRepository(GridironDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<List<Team>> GetAllAsync()
    {
        return await _context.Teams.ToListAsync();
    }

    public async Task<Team?> GetByIdAsync(int teamId)
    {
        return await _context.Teams.FindAsync(teamId);
    }

    public async Task<Team?> GetByIdWithPlayersAsync(int teamId)
    {
        return await _context.Teams
            .Include(t => t.Players)
            .FirstOrDefaultAsync(t => t.Id == teamId);
    }

    public async Task<Team?> GetByCityAndNameAsync(string city, string name)
    {
        return await _context.Teams
            .Include(t => t.Players)
            .FirstOrDefaultAsync(t => t.City == city && t.Name == name);
    }

    public async Task<Team> AddAsync(Team team)
    {
        await _context.Teams.AddAsync(team);
        await _context.SaveChangesAsync();
        return team;
    }

    public async Task UpdateAsync(Team team)
    {
        _context.Teams.Update(team);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int teamId)
    {
        var team = await _context.Teams.FindAsync(teamId);
        if (team != null)
        {
            _context.Teams.Remove(team);
            await _context.SaveChangesAsync();
        }
    }

    public async Task SoftDeleteAsync(int teamId, string? deletedBy = null, string? reason = null)
    {
        var team = await _context.Teams
            .IgnoreQueryFilters()  // Include soft-deleted entities
            .FirstOrDefaultAsync(t => t.Id == teamId);

        if (team == null)
            throw new InvalidOperationException($"Team with ID {teamId} not found");

        if (team.IsDeleted)
            throw new InvalidOperationException($"Team with ID {teamId} is already deleted");

        team.SoftDelete(deletedBy, reason);
        await _context.SaveChangesAsync();
    }

    public async Task RestoreAsync(int teamId)
    {
        var team = await _context.Teams
            .IgnoreQueryFilters()  // Include soft-deleted entities
            .FirstOrDefaultAsync(t => t.Id == teamId);

        if (team == null)
            throw new InvalidOperationException($"Team with ID {teamId} not found");

        if (!team.IsDeleted)
            throw new InvalidOperationException($"Team with ID {teamId} is not deleted");

        team.Restore();
        await _context.SaveChangesAsync();
    }

    public async Task<List<Team>> GetDeletedAsync()
    {
        return await _context.Teams
            .IgnoreQueryFilters()  // Include soft-deleted entities
            .Where(t => t.IsDeleted)
            .ToListAsync();
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}

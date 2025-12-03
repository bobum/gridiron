using DomainObjects;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories;

/// <summary>
/// Repository implementation for User data access
/// ALL database access for users goes through this class.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly GridironDbContext _context;

    public UserRepository(GridironDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<User?> GetByAzureAdObjectIdAsync(string azureAdObjectId)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.AzureAdObjectId == azureAdObjectId);
    }

    public async Task<User?> GetByIdAsync(int userId)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<User?> GetByIdWithRolesAsync(int userId)
    {
        return await _context.Users
            .Include(u => u.LeagueRoles)
                .ThenInclude(ulr => ulr.League)
            .Include(u => u.LeagueRoles)
                .ThenInclude(ulr => ulr.Team)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<User?> GetByAzureAdObjectIdWithRolesAsync(string azureAdObjectId)
    {
        return await _context.Users
            .Include(u => u.LeagueRoles)
                .ThenInclude(ulr => ulr.League)
            .Include(u => u.LeagueRoles)
                .ThenInclude(ulr => ulr.Team)
            .FirstOrDefaultAsync(u => u.AzureAdObjectId == azureAdObjectId);
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _context.Users.ToListAsync();
    }

    public async Task<User> AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task<List<User>> GetGlobalAdminsAsync()
    {
        return await _context.Users
            .Where(u => u.IsGlobalAdmin)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(string azureAdObjectId)
    {
        return await _context.Users
            .AnyAsync(u => u.AzureAdObjectId == azureAdObjectId);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}

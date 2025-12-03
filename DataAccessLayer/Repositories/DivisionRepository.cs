using DomainObjects;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories;

/// <summary>
/// Repository implementation for Division data access
/// ALL database access for divisions goes through this class.
/// </summary>
public class DivisionRepository : IDivisionRepository
{
    private readonly GridironDbContext _context;

    public DivisionRepository(GridironDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<List<Division>> GetAllAsync()
    {
        return await _context.Divisions.ToListAsync();
    }

    public async Task<Division?> GetByIdAsync(int divisionId)
    {
        return await _context.Divisions.FirstOrDefaultAsync(d => d.Id == divisionId);
    }

    public async Task<Division?> GetByIdWithTeamsAsync(int divisionId)
    {
        return await _context.Divisions
            .Include(d => d.Teams)
            .FirstOrDefaultAsync(d => d.Id == divisionId);
    }

    public async Task<Division> AddAsync(Division division)
    {
        await _context.Divisions.AddAsync(division);
        await _context.SaveChangesAsync();
        return division;
    }

    public async Task UpdateAsync(Division division)
    {
        _context.Divisions.Update(division);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int divisionId)
    {
        var division = await _context.Divisions.FindAsync(divisionId);
        if (division != null)
        {
            _context.Divisions.Remove(division);
            await _context.SaveChangesAsync();
        }
    }

    public async Task SoftDeleteAsync(int divisionId, string? deletedBy = null, string? reason = null)
    {
        var division = await _context.Divisions
            .IgnoreQueryFilters() // Include soft-deleted entities
            .FirstOrDefaultAsync(d => d.Id == divisionId);

        if (division == null)
        {
            throw new InvalidOperationException($"Division with ID {divisionId} not found");
        }

        if (division.IsDeleted)
        {
            throw new InvalidOperationException($"Division with ID {divisionId} is already deleted");
        }

        division.SoftDelete(deletedBy, reason);
        await _context.SaveChangesAsync();
    }

    public async Task RestoreAsync(int divisionId)
    {
        var division = await _context.Divisions
            .IgnoreQueryFilters() // Include soft-deleted entities
            .FirstOrDefaultAsync(d => d.Id == divisionId);

        if (division == null)
        {
            throw new InvalidOperationException($"Division with ID {divisionId} not found");
        }

        if (!division.IsDeleted)
        {
            throw new InvalidOperationException($"Division with ID {divisionId} is not deleted");
        }

        division.Restore();
        await _context.SaveChangesAsync();
    }

    public async Task<List<Division>> GetDeletedAsync()
    {
        return await _context.Divisions
            .IgnoreQueryFilters() // Include soft-deleted entities
            .Where(d => d.IsDeleted)
            .ToListAsync();
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}

using DomainObjects;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories;

/// <summary>
/// Repository implementation for Conference data access
/// ALL database access for conferences goes through this class
/// </summary>
public class ConferenceRepository : IConferenceRepository
{
    private readonly GridironDbContext _context;

    public ConferenceRepository(GridironDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<List<Conference>> GetAllAsync()
    {
        return await _context.Conferences.ToListAsync();
    }

    public async Task<Conference?> GetByIdAsync(int conferenceId)
    {
        return await _context.Conferences.FirstOrDefaultAsync(c => c.Id == conferenceId);
    }

    public async Task<Conference?> GetByIdWithDivisionsAsync(int conferenceId)
    {
        return await _context.Conferences
            .Include(c => c.Divisions)
                .ThenInclude(d => d.Teams)
            .FirstOrDefaultAsync(c => c.Id == conferenceId);
    }

    public async Task<Conference> AddAsync(Conference conference)
    {
        await _context.Conferences.AddAsync(conference);
        await _context.SaveChangesAsync();
        return conference;
    }

    public async Task UpdateAsync(Conference conference)
    {
        _context.Conferences.Update(conference);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int conferenceId)
    {
        var conference = await _context.Conferences.FindAsync(conferenceId);
        if (conference != null)
        {
            _context.Conferences.Remove(conference);
            await _context.SaveChangesAsync();
        }
    }

    public async Task SoftDeleteAsync(int conferenceId, string? deletedBy = null, string? reason = null)
    {
        var conference = await _context.Conferences
            .IgnoreQueryFilters()  // Include soft-deleted entities
            .FirstOrDefaultAsync(c => c.Id == conferenceId);

        if (conference == null)
            throw new InvalidOperationException($"Conference with ID {conferenceId} not found");

        if (conference.IsDeleted)
            throw new InvalidOperationException($"Conference with ID {conferenceId} is already deleted");

        conference.SoftDelete(deletedBy, reason);
        await _context.SaveChangesAsync();
    }

    public async Task RestoreAsync(int conferenceId)
    {
        var conference = await _context.Conferences
            .IgnoreQueryFilters()  // Include soft-deleted entities
            .FirstOrDefaultAsync(c => c.Id == conferenceId);

        if (conference == null)
            throw new InvalidOperationException($"Conference with ID {conferenceId} not found");

        if (!conference.IsDeleted)
            throw new InvalidOperationException($"Conference with ID {conferenceId} is not deleted");

        conference.Restore();
        await _context.SaveChangesAsync();
    }

    public async Task<List<Conference>> GetDeletedAsync()
    {
        return await _context.Conferences
            .IgnoreQueryFilters()  // Include soft-deleted entities
            .Where(c => c.IsDeleted)
            .ToListAsync();
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}

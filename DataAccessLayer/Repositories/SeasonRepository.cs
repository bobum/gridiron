using DomainObjects;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories;

/// <summary>
/// Repository implementation for Season data access
/// ALL database access for seasons goes through this class.
/// </summary>
public class SeasonRepository : ISeasonRepository
{
    private readonly GridironDbContext _context;

    public SeasonRepository(GridironDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<List<Season>> GetAllAsync()
    {
        return await _context.Seasons.ToListAsync();
    }

    public async Task<Season?> GetByIdAsync(int seasonId)
    {
        return await _context.Seasons.FirstOrDefaultAsync(s => s.Id == seasonId);
    }

    public async Task<Season?> GetByIdWithWeeksAsync(int seasonId)
    {
        return await _context.Seasons
            .Include(s => s.Weeks)
            .FirstOrDefaultAsync(s => s.Id == seasonId);
    }

    public async Task<Season?> GetByIdWithWeeksAndGamesAsync(int seasonId)
    {
        return await _context.Seasons
            .Include(s => s.Weeks)
                .ThenInclude(w => w.Games)
            .FirstOrDefaultAsync(s => s.Id == seasonId);
    }

    public async Task<Season?> GetByIdWithFullDataAsync(int seasonId)
    {
        return await _context.Seasons
            .Include(s => s.League)
            .Include(s => s.Weeks)
                .ThenInclude(w => w.Games)
                    .ThenInclude(g => g.HomeTeam)
            .Include(s => s.Weeks)
                .ThenInclude(w => w.Games)
                    .ThenInclude(g => g.AwayTeam)
            .Include(s => s.ChampionTeam)
            .FirstOrDefaultAsync(s => s.Id == seasonId);
    }

    public async Task<Season?> GetByIdWithCurrentWeekAsync(int seasonId)
    {
        // First get the season to know the current week
        var season = await _context.Seasons.FirstOrDefaultAsync(s => s.Id == seasonId);
        if (season == null) return null;

        // Load the current week
        await _context.Entry(season)
            .Collection(s => s.Weeks)
            .Query()
            .Where(w => w.WeekNumber == season.CurrentWeek)
            .Include(w => w.Games)
                .ThenInclude(g => g.HomeTeam)
            .Include(w => w.Games)
                .ThenInclude(g => g.AwayTeam)
            .LoadAsync();

        return season;
    }


    public async Task<List<Season>> GetByLeagueIdAsync(int leagueId)
    {
        return await _context.Seasons
            .Where(s => s.LeagueId == leagueId)
            .OrderByDescending(s => s.Year)
            .ToListAsync();
    }

    public async Task<Season?> GetCurrentSeasonForLeagueAsync(int leagueId)
    {
        var league = await _context.Leagues
            .Include(l => l.CurrentSeason)
                .ThenInclude(s => s!.Weeks)
            .FirstOrDefaultAsync(l => l.Id == leagueId);

        return league?.CurrentSeason;
    }

    public async Task<Season> AddAsync(Season season)
    {
        await _context.Seasons.AddAsync(season);
        await _context.SaveChangesAsync();
        return season;
    }

    public async Task UpdateAsync(Season season)
    {
        _context.Seasons.Update(season);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int seasonId)
    {
        var season = await _context.Seasons.FindAsync(seasonId);
        if (season != null)
        {
            _context.Seasons.Remove(season);
            await _context.SaveChangesAsync();
        }
    }

    public async Task SoftDeleteAsync(int seasonId, string? deletedBy = null, string? reason = null)
    {
        var season = await _context.Seasons
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == seasonId);

        if (season == null)
        {
            throw new InvalidOperationException($"Season with ID {seasonId} not found");
        }

        if (season.IsDeleted)
        {
            throw new InvalidOperationException($"Season with ID {seasonId} is already deleted");
        }

        season.SoftDelete(deletedBy, reason);
        await _context.SaveChangesAsync();
    }

    public async Task RestoreAsync(int seasonId)
    {
        var season = await _context.Seasons
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.Id == seasonId);

        if (season == null)
        {
            throw new InvalidOperationException($"Season with ID {seasonId} not found");
        }

        if (!season.IsDeleted)
        {
            throw new InvalidOperationException($"Season with ID {seasonId} is not deleted");
        }

        season.Restore();
        await _context.SaveChangesAsync();
    }

    public async Task<List<Season>> GetDeletedAsync()
    {
        return await _context.Seasons
            .IgnoreQueryFilters()
            .Where(s => s.IsDeleted)
            .ToListAsync();
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}

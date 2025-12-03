using DomainObjects;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories;

/// <summary>
/// Repository implementation for League data access
/// ALL database access for leagues goes through this class.
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
            .IgnoreQueryFilters() // Include soft-deleted entities
            .FirstOrDefaultAsync(l => l.Id == leagueId);

        if (league == null)
        {
            throw new InvalidOperationException($"League with ID {leagueId} not found");
        }

        if (league.IsDeleted)
        {
            throw new InvalidOperationException($"League with ID {leagueId} is already deleted");
        }

        league.SoftDelete(deletedBy, reason);
        await _context.SaveChangesAsync();
    }

    public async Task RestoreAsync(int leagueId)
    {
        var league = await _context.Leagues
            .IgnoreQueryFilters() // Include soft-deleted entities
            .FirstOrDefaultAsync(l => l.Id == leagueId);

        if (league == null)
        {
            throw new InvalidOperationException($"League with ID {leagueId} not found");
        }

        if (!league.IsDeleted)
        {
            throw new InvalidOperationException($"League with ID {leagueId} is not deleted");
        }

        league.Restore();
        await _context.SaveChangesAsync();
    }

    public async Task<List<League>> GetDeletedAsync()
    {
        return await _context.Leagues
            .IgnoreQueryFilters() // Include soft-deleted entities
            .Where(l => l.IsDeleted)
            .ToListAsync();
    }

    public async Task<CascadeDeleteResult> SoftDeleteWithCascadeAsync(int leagueId, string? deletedBy = null, string? reason = null)
    {
        var result = new CascadeDeleteResult
        {
            DeletedBy = deletedBy,
            DeletionReason = reason,
            DeletedAt = DateTime.UtcNow
        };

        try
        {
            // Start transaction for atomic cascade delete
            using var transaction = await _context.Database.BeginTransactionAsync();

            // Get league with full structure (need to use IgnoreQueryFilters to include already soft-deleted children)
            var league = await _context.Leagues
                .IgnoreQueryFilters()
                .Include(l => l.Conferences)
                    .ThenInclude(c => c.Divisions)
                        .ThenInclude(d => d.Teams)
                            .ThenInclude(t => t.Players)
                .FirstOrDefaultAsync(l => l.Id == leagueId);

            if (league == null)
            {
                result.Success = false;
                result.ErrorMessage = $"League with ID {leagueId} not found";
                return result;
            }

            if (league.IsDeleted)
            {
                result.Success = false;
                result.ErrorMessage = $"League with ID {leagueId} is already deleted";
                return result;
            }

            // Track counts by entity type
            int leagueCount = 0, conferenceCount = 0, divisionCount = 0, teamCount = 0, playerCount = 0;
            var leagueIds = new List<int>();
            var conferenceIds = new List<int>();
            var divisionIds = new List<int>();
            var teamIds = new List<int>();
            var playerIds = new List<int>();

            // Cascade delete: League → Conferences → Divisions → Teams → Players
            foreach (var conference in league.Conferences)
            {
                if (!conference.IsDeleted)
                {
                    foreach (var division in conference.Divisions)
                    {
                        if (!division.IsDeleted)
                        {
                            foreach (var team in division.Teams)
                            {
                                if (!team.IsDeleted)
                                {
                                    // Soft delete players
                                    foreach (var player in team.Players)
                                    {
                                        if (!player.IsDeleted)
                                        {
                                            player.SoftDelete(deletedBy, $"Cascade from Team {team.Id}: {reason}");
                                            playerCount++;
                                            playerIds.Add(player.Id);
                                        }
                                    }

                                    // Soft delete team
                                    team.SoftDelete(deletedBy, $"Cascade from Division {division.Id}: {reason}");
                                    teamCount++;
                                    teamIds.Add(team.Id);
                                }
                            }

                            // Soft delete division
                            division.SoftDelete(deletedBy, $"Cascade from Conference {conference.Id}: {reason}");
                            divisionCount++;
                            divisionIds.Add(division.Id);
                        }
                    }

                    // Soft delete conference
                    conference.SoftDelete(deletedBy, $"Cascade from League {league.Id}: {reason}");
                    conferenceCount++;
                    conferenceIds.Add(conference.Id);
                }
            }

            // Soft delete league
            league.SoftDelete(deletedBy, reason);
            leagueCount = 1;
            leagueIds.Add(league.Id);

            // Save all changes
            await _context.SaveChangesAsync();

            // Commit transaction
            await transaction.CommitAsync();

            // Populate result
            result.TotalEntitiesDeleted = leagueCount + conferenceCount + divisionCount + teamCount + playerCount;
            result.DeletedByType["Leagues"] = leagueCount;
            result.DeletedByType["Conferences"] = conferenceCount;
            result.DeletedByType["Divisions"] = divisionCount;
            result.DeletedByType["Teams"] = teamCount;
            result.DeletedByType["Players"] = playerCount;

            result.DeletedIds["Leagues"] = leagueIds;
            result.DeletedIds["Conferences"] = conferenceIds;
            result.DeletedIds["Divisions"] = divisionIds;
            result.DeletedIds["Teams"] = teamIds;
            result.DeletedIds["Players"] = playerIds;

            // Add warning about preserved historical data
            var gamesCount = await _context.Games
                .Where(g => teamIds.Contains(g.HomeTeamId) || teamIds.Contains(g.AwayTeamId))
                .CountAsync();

            if (gamesCount > 0)
            {
                result.Warnings.Add($"{gamesCount} games involving deleted teams are preserved as historical data");
            }

            result.Success = true;
            return result;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = $"Cascade delete failed: {ex.Message}";
            return result;
        }
    }

    public async Task<CascadeRestoreResult> RestoreWithCascadeAsync(int leagueId, bool cascade = false)
    {
        var result = new CascadeRestoreResult
        {
            RestoredAt = DateTime.UtcNow
        };

        try
        {
            // Start transaction for atomic cascade restore
            using var transaction = await _context.Database.BeginTransactionAsync();

            // Get league with full structure (include soft-deleted entities)
            var league = await _context.Leagues
                .IgnoreQueryFilters()
                .Include(l => l.Conferences)
                    .ThenInclude(c => c.Divisions)
                        .ThenInclude(d => d.Teams)
                            .ThenInclude(t => t.Players)
                .FirstOrDefaultAsync(l => l.Id == leagueId);

            if (league == null)
            {
                result.Success = false;
                result.ErrorMessage = $"League with ID {leagueId} not found";
                return result;
            }

            if (!league.IsDeleted)
            {
                result.Success = false;
                result.ErrorMessage = $"League with ID {leagueId} is not deleted";
                return result;
            }

            // Track counts
            int leagueCount = 0, conferenceCount = 0, divisionCount = 0, teamCount = 0, playerCount = 0;
            var leagueIds = new List<int>();
            var conferenceIds = new List<int>();
            var divisionIds = new List<int>();
            var teamIds = new List<int>();
            var playerIds = new List<int>();

            // Restore league
            league.Restore();
            leagueCount = 1;
            leagueIds.Add(league.Id);

            if (cascade)
            {
                // Cascade restore: League → Conferences → Divisions → Teams → Players
                foreach (var conference in league.Conferences)
                {
                    if (conference.IsDeleted)
                    {
                        conference.Restore();
                        conferenceCount++;
                        conferenceIds.Add(conference.Id);

                        foreach (var division in conference.Divisions)
                        {
                            if (division.IsDeleted)
                            {
                                division.Restore();
                                divisionCount++;
                                divisionIds.Add(division.Id);

                                foreach (var team in division.Teams)
                                {
                                    if (team.IsDeleted)
                                    {
                                        team.Restore();
                                        teamCount++;
                                        teamIds.Add(team.Id);

                                        foreach (var player in team.Players)
                                        {
                                            if (player.IsDeleted)
                                            {
                                                player.Restore();
                                                playerCount++;
                                                playerIds.Add(player.Id);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                // Count orphaned children that will remain deleted
                var deletedConferences = league.Conferences.Count(c => c.IsDeleted);
                var deletedDivisions = league.Conferences.SelectMany(c => c.Divisions).Count(d => d.IsDeleted);
                var deletedTeams = league.Conferences.SelectMany(c => c.Divisions).SelectMany(d => d.Teams).Count(t => t.IsDeleted);

                if (deletedConferences > 0)
                {
                    result.Warnings.Add($"{deletedConferences} conferences remain soft-deleted");
                }

                if (deletedDivisions > 0)
                {
                    result.Warnings.Add($"{deletedDivisions} divisions remain soft-deleted");
                }

                if (deletedTeams > 0)
                {
                    result.Warnings.Add($"{deletedTeams} teams remain soft-deleted");
                }
            }

            // Save changes
            await _context.SaveChangesAsync();

            // Commit transaction
            await transaction.CommitAsync();

            // Populate result
            result.TotalEntitiesRestored = leagueCount + conferenceCount + divisionCount + teamCount + playerCount;
            if (leagueCount > 0)
            {
                result.RestoredByType["Leagues"] = leagueCount;
            }

            if (conferenceCount > 0)
            {
                result.RestoredByType["Conferences"] = conferenceCount;
            }

            if (divisionCount > 0)
            {
                result.RestoredByType["Divisions"] = divisionCount;
            }

            if (teamCount > 0)
            {
                result.RestoredByType["Teams"] = teamCount;
            }

            if (playerCount > 0)
            {
                result.RestoredByType["Players"] = playerCount;
            }

            if (leagueIds.Any())
            {
                result.RestoredIds["Leagues"] = leagueIds;
            }

            if (conferenceIds.Any())
            {
                result.RestoredIds["Conferences"] = conferenceIds;
            }

            if (divisionIds.Any())
            {
                result.RestoredIds["Divisions"] = divisionIds;
            }

            if (teamIds.Any())
            {
                result.RestoredIds["Teams"] = teamIds;
            }

            if (playerIds.Any())
            {
                result.RestoredIds["Players"] = playerIds;
            }

            result.Success = true;
            return result;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = $"Cascade restore failed: {ex.Message}";
            return result;
        }
    }

    public async Task<RestoreValidationResult> ValidateRestoreAsync(int leagueId)
    {
        var result = new RestoreValidationResult
        {
            CanRestore = true
        };

        // Get league (include soft-deleted)
        var league = await _context.Leagues
            .IgnoreQueryFilters()
            .Include(l => l.Conferences)
                .ThenInclude(c => c.Divisions)
                    .ThenInclude(d => d.Teams)
            .FirstOrDefaultAsync(l => l.Id == leagueId);

        if (league == null)
        {
            result.CanRestore = false;
            result.ValidationErrors.Add($"League with ID {leagueId} not found");
            return result;
        }

        if (!league.IsDeleted)
        {
            result.CanRestore = false;
            result.ValidationErrors.Add($"League with ID {leagueId} is not deleted");
            return result;
        }

        // League has no parents, so it can always be restored
        // Check for orphaned children
        var deletedConferences = league.Conferences.Count(c => c.IsDeleted);
        var deletedDivisions = league.Conferences.SelectMany(c => c.Divisions).Count(d => d.IsDeleted);
        var deletedTeams = league.Conferences.SelectMany(c => c.Divisions).SelectMany(d => d.Teams).Count(t => t.IsDeleted);

        if (deletedConferences > 0)
        {
            result.OrphanedChildren["Conferences"] = deletedConferences;
        }

        if (deletedDivisions > 0)
        {
            result.OrphanedChildren["Divisions"] = deletedDivisions;
        }

        if (deletedTeams > 0)
        {
            result.OrphanedChildren["Teams"] = deletedTeams;
        }

        if (result.OrphanedChildren.Any())
        {
            result.Warnings.Add("Some child entities will remain soft-deleted. Consider using cascade restore.");
        }

        return result;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}

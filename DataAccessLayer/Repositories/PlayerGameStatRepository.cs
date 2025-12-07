using DomainObjects;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccessLayer.Repositories
{
    public class PlayerGameStatRepository : IPlayerGameStatRepository
    {
        private readonly GridironDbContext _context;

        public PlayerGameStatRepository(GridironDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(PlayerGameStat stat)
        {
            await _context.PlayerGameStats.AddAsync(stat);
            await _context.SaveChangesAsync();
        }

        public async Task AddRangeAsync(IEnumerable<PlayerGameStat> stats)
        {
            await _context.PlayerGameStats.AddRangeAsync(stats);
            await _context.SaveChangesAsync();
        }

        public async Task<List<PlayerGameStat>> GetByGameIdAsync(int gameId)
        {
            return await _context.PlayerGameStats
                .Where(pgs => pgs.GameId == gameId)
                .ToListAsync();
        }

        public async Task DeleteByGameIdAsync(int gameId)
        {
            var stats = await _context.PlayerGameStats
                .Where(pgs => pgs.GameId == gameId)
                .ToListAsync();

            if (stats.Any())
            {
                foreach (var stat in stats)
                {
                    stat.IsDeleted = true;
                    stat.DeletedAt = System.DateTime.UtcNow;
                }
                await _context.SaveChangesAsync();
            }
        }
    }
}

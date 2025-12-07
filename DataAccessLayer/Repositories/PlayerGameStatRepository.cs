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
        }

        public async Task AddRangeAsync(IEnumerable<PlayerGameStat> stats)
        {
            await _context.PlayerGameStats.AddRangeAsync(stats);
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
                _context.PlayerGameStats.RemoveRange(stats);
            }
        }
    }
}

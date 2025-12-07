using DomainObjects;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccessLayer.Repositories
{
    public interface IPlayerGameStatRepository
    {
        Task AddAsync(PlayerGameStat stat);
        Task AddRangeAsync(IEnumerable<PlayerGameStat> stats);
        Task<List<PlayerGameStat>> GetByGameIdAsync(int gameId);
        Task DeleteByGameIdAsync(int gameId);
    }
}

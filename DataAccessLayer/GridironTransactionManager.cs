using Microsoft.EntityFrameworkCore.Storage;

namespace DataAccessLayer;

public class GridironTransactionManager : ITransactionManager
{
    private readonly GridironDbContext _context;

    public GridironTransactionManager(GridironDbContext context)
    {
        _context = context;
    }

    public Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return _context.Database.BeginTransactionAsync();
    }
}

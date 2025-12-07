using Microsoft.EntityFrameworkCore.Storage;

namespace DataAccessLayer;

public interface ITransactionManager
{
    Task<IDbContextTransaction> BeginTransactionAsync();
}

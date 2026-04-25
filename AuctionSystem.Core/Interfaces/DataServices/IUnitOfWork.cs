using System.Data;

namespace AuctionSystem.Core.Interfaces.Services;

public interface IUnitOfWork
{
    IDbTransaction? Transaction { get; }
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
}

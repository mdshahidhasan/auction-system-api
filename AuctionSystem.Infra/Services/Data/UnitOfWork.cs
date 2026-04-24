using System.Data;
using AuctionSystem.Core.Interfaces.Services;
using MySqlConnector;

namespace AuctionSystem.Infra.Services.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly MySqlConnection _connection;

    public IDbTransaction? Transaction { get; private set; }

    public UnitOfWork(MySqlConnection connection)
    {
        _connection = connection;
    }

    public async Task BeginTransactionAsync()
    {
        if (Transaction is not null)
        {
            return;
        }

        if (_connection.State != ConnectionState.Open)
        {
            await _connection.OpenAsync();
        }

        Transaction = await _connection.BeginTransactionAsync();
    }

    public async Task CommitAsync()
    {
        if (Transaction is null)
        {
            return;
        }

        await ((MySqlTransaction)Transaction).CommitAsync();
        await DisposeTransactionAsync();
    }

    public async Task RollbackAsync()
    {
        if (Transaction is null)
        {
            return;
        }

        await ((MySqlTransaction)Transaction).RollbackAsync();
        await DisposeTransactionAsync();
    }

    private async Task DisposeTransactionAsync()
    {
        Transaction?.Dispose();
        Transaction = null;

        if (_connection.State == ConnectionState.Open)
        {
            await _connection.CloseAsync();
        }
    }
}

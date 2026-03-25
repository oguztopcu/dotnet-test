using System.Data;
using AcilEvrak.Domain.Interfaces;

namespace AcilEvrak.Infrastructure.Database;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly IDbConnectionFactory _connectionFactory;
    private IDbConnection? _connection;
    private IDbTransaction? _transaction;

    public IDbConnection Connection => _connection ?? throw new InvalidOperationException("UnitOfWork has not been started.");
    public IDbTransaction Transaction => _transaction ?? throw new InvalidOperationException("UnitOfWork has not been started.");

    public UnitOfWork(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task BeginAsync(CancellationToken cancellationToken = default)
    {
        _connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        _transaction = _connection.BeginTransaction();
    }

    public Task CommitAsync(CancellationToken cancellationToken = default)
    {
        _transaction?.Commit();
        return Task.CompletedTask;
    }

    public Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        _transaction?.Rollback();
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaction is IAsyncDisposable asyncTransaction)
            await asyncTransaction.DisposeAsync();
        else
            _transaction?.Dispose();

        if (_connection is IAsyncDisposable asyncConnection)
            await asyncConnection.DisposeAsync();
        else
            _connection?.Dispose();

        _transaction = null;
        _connection = null;
    }
}

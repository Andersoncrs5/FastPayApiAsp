using App.Config.Database;
using App.Config.Database.Session;
using Npgsql;

namespace App.Config.Tx;

public sealed class RequestDbContext(
    IDatabase database)
    : IDatabaseSession, IRequestDbContext
{
    private NpgsqlConnection? _connection;
    private NpgsqlTransaction? _transaction;

    public NpgsqlConnection Connection =>
        _connection ??
        throw new InvalidOperationException(
            "Connection was not initialized.");

    public NpgsqlTransaction Transaction =>
        _transaction ??
        throw new InvalidOperationException(
            "Transaction was not initialized.");

    public async Task BeginAsync(
        CancellationToken cancellationToken = default)
    {
        if (_connection is not null)
            return;

        _connection =
            await database.OpenConnectionAsync(cancellationToken);

        _transaction =
            await _connection.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitAsync(
        CancellationToken cancellationToken = default)
    {
        if (_transaction is null)
            return;

        await _transaction.CommitAsync(cancellationToken);

        await DisposeAsync();
    }

    public async Task RollbackAsync(
        CancellationToken cancellationToken = default)
    {
        if (_transaction is null)
            return;

        await _transaction.RollbackAsync(cancellationToken);

        await DisposeAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_transaction is not null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        if (_connection is not null)
        {
            await _connection.DisposeAsync();
            _connection = null;
        }
    }
}
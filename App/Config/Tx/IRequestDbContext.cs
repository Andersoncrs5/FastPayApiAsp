using App.Config.Database.Session;
using Npgsql;

namespace App.Config.Tx;

public interface IRequestDbContext : IAsyncDisposable, IDatabaseSession
{
    Task BeginAsync(
        CancellationToken cancellationToken = default);

    Task CommitAsync(
        CancellationToken cancellationToken = default);

    Task RollbackAsync(
        CancellationToken cancellationToken = default);
}
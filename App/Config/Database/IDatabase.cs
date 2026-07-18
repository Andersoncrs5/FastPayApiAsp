using Npgsql;

namespace App.Config.Database;

public interface IDatabase
{
    ValueTask<NpgsqlConnection> OpenConnectionAsync(
        CancellationToken cancellationToken = default);
}
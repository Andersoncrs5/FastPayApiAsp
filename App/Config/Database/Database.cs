using App.Config.Options;
using Microsoft.Extensions.Options;
using Npgsql;

namespace App.Config.Database;

public sealed class Database(IOptions<DatabaseOptions> options) : IDatabase
{
    public async ValueTask<NpgsqlConnection> OpenConnectionAsync(
        CancellationToken cancellationToken = default)
    {
        var connection =
            new NpgsqlConnection(options.Value.Postgres);

        await connection.OpenAsync(cancellationToken);

        return connection;
    }
}
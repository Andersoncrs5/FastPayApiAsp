using Dapper;
using Npgsql;

namespace App.Config.Database.Migrations;

public sealed class V001CreateSchemaMigrationsTable : IMigration
{
    public int Version => 1;

    public string Name => "Create schema_migrations";

    public async Task UpAsync(
        NpgsqlConnection connection,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
                           CREATE TABLE IF NOT EXISTS schema_migrations
                           (
                               version INTEGER PRIMARY KEY,
                               name TEXT NOT NULL,
                               executed_at TIMESTAMPTZ NOT NULL
                           );
                           """;

        await connection.ExecuteAsync(sql);
    }
}
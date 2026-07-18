using Dapper;
using Npgsql;

namespace App.Config.Database;

public sealed class MigrationRunner(
    IEnumerable<IMigration> migrations,
    IDatabase database)
{
    public async Task RunAsync()
    {
        await using var connection = await database.OpenConnectionAsync();

        await connection.OpenAsync();


        foreach (var migration in migrations.OrderBy(x => x.Version))
        {
            var executed = await connection.ExecuteScalarAsync<int?>(
                """
                SELECT version
                FROM schema_migrations
                WHERE version = @Version
                """,
                new
                {
                    Version = migration.Version
                });


            if (executed.HasValue)
                continue;


            await migration.UpAsync(connection);


            await connection.ExecuteAsync(
                """
                INSERT INTO schema_migrations
                (
                    version,
                    name,
                    executed_at
                )
                VALUES
                (
                    @Version,
                    @Name,
                    NOW()
                )
                """,
                new
                {
                    Version = migration.Version,
                    Name = migration.Name
                });
        }
    }
}
using Npgsql;

namespace App.Config.Database;

public interface IMigration
{
    int Version { get; }

    string Name { get; }

    Task UpAsync(
        NpgsqlConnection connection,
        CancellationToken cancellationToken = default
    );
}
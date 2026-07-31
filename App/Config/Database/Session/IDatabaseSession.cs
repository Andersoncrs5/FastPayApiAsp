using Npgsql;

namespace App.Config.Database.Session;

public interface IDatabaseSession : IAsyncDisposable
{
    NpgsqlConnection Connection { get; }

    NpgsqlTransaction? Transaction { get; }
}
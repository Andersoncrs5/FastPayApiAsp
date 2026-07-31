using App.Config.Database;
using App.Config.Database.Session;
using Npgsql;

namespace App.Tests.Config;

public sealed class TestDbSession : IDatabaseSession
{
    private readonly NpgsqlConnection _connection;


    public TestDbSession(IDatabase database)
    {
        _connection = database
            .OpenConnectionAsync()
            .GetAwaiter()
            .GetResult();
    }

    public NpgsqlConnection Connection => _connection;


    public NpgsqlTransaction? Transaction => null;


    public ValueTask DisposeAsync()
    {
        return _connection.DisposeAsync();
    }
}
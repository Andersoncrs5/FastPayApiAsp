using StackExchange.Redis;

namespace App.Config.Redis;

public sealed class RedisDatabase(
    IConnectionMultiplexer multiplexer)
    : IRedisDatabase
{
    public IDatabase Db => multiplexer.GetDatabase();
}
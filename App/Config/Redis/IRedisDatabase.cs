using StackExchange.Redis;

namespace App.Config.Redis;

public interface IRedisDatabase
{
    IDatabase Db { get; }
}
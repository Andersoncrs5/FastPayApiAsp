using IdGen;

namespace App.Config.Snowflake;

public sealed class SnowflakeGenerator(
    IIdGenerator<long> generator)
    : ISnowflakeGenerator
{
    public long Next() => generator.CreateId();
}
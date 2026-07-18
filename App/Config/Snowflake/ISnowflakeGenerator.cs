namespace App.Config.Snowflake;

public interface ISnowflakeGenerator
{
    long Next();
}
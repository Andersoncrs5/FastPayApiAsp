namespace App.Config.Options;

public sealed class DatabaseOptions
{
    public const string SectionName = "ConnectionStrings";

    public required string Postgres { get; init; }
}
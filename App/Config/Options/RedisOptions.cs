using System.ComponentModel.DataAnnotations;

namespace App.Config.Options;

public sealed class RedisOptions
{
    public const string SectionName = "Redis";

    [Required]
    public required string ConnectionString { get; init; }

    public string InstanceName { get; init; } = "FastPay";
}
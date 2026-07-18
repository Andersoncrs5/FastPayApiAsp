using System.ComponentModel.DataAnnotations;

namespace App.Config.Options;

public sealed class RedisOptions
{
    public const string SectionName = "Redis";

    [Required]
    public required string ConnectionString { get; set; }

    public string InstanceName { get; set; } = "FastPay";
}
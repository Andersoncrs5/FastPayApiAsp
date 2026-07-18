using System.ComponentModel.DataAnnotations;

namespace App.Config.Options;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    public required string SecretKey { get; init; }

    [Required]
    public required string ValidIssuer { get; init; }

    [Required]
    public required string ValidAudience { get; init; }

    [Range(1, int.MaxValue)]
    public int TokenValidityInMinutes { get; init; }

    [Range(1, int.MaxValue)]
    public int RefreshTokenValidityInMinutes { get; init; }
}
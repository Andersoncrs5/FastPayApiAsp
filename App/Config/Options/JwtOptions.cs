using System.ComponentModel.DataAnnotations;

namespace App.Config.Options;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    public required string SecretKey { get; set; }

    [Required]
    public required string ValidIssuer { get; set; }

    [Required]
    public required string ValidAudience { get; set; }

    [Range(1, int.MaxValue)]
    public int TokenValidityInMinutes { get; set; }

    [Range(1, int.MaxValue)]
    public int RefreshTokenValidityInMinutes { get; set; }
}
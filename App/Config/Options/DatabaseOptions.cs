using System.ComponentModel.DataAnnotations;

namespace App.Config.Options;

public sealed class DatabaseOptions
{
    public const string SectionName = "ConnectionStrings";

    [Required(ErrorMessage = "Database name is required")]
    public required string Postgres { get; set; }
}
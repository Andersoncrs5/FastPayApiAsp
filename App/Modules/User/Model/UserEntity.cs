using App.Utils.Base.Entity;
namespace App.Modules.User.Model;

public sealed class UserEntity: BaseEntity
{
    public const string _tableName = "users";

    public string UserName { get; set; } = null!;

    public string NormalizedUserName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string NormalizedEmail { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public bool Active { get; set; } = true;


    public DateTimeOffset? LastLoginAt { get; set; }
}    

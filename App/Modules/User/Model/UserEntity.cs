using Microsoft.AspNetCore.Identity;

namespace App.Modules.User.Model;

public class UserEntity : IdentityUser<long>
{       
    public string FullName { get; set; } = null!;

    public bool Active { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public DateTimeOffset? LastLoginAt { get; set; }
    
}
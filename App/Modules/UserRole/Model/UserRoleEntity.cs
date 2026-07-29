using App.Modules.Role.Model;
using App.Modules.User.Model;
using App.Utils.Base.Entity;

namespace App.Modules.UserRole.Model;

public class UserRoleEntity: BaseEntity
{
    public const string _tableName = "user_roles";
    
    public required long RoleId { get; set; }
    
    public required long UserId { get; set; }
    
    public bool Active { get; set; }
    
    public long? AssignedByUserId { get; set; } 
    
    public DateTimeOffset? RevokedAt { get; set; }
    
}
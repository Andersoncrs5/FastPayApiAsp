using App.Utils.Base.Entity;

namespace App.Modules.Role.Model;

public class RoleEntity: BaseEntity
{
    public const string _tableName = "roles";
    
    public required string Name { get; set; }
    public required string NormalizedName { get; set; }
    public string? Description { get; set; }
    public bool Active { get; set; }
}
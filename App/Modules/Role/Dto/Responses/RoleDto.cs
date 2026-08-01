using App.Utils.Base.Dto;

namespace App.Modules.Role.Dto.Responses;

public class RoleDto: BaseDto
{
    public required string Name { get; set; }
    public required string NormalizedName { get; set; }
    public string? Description { get; set; }
    public bool Active { get; set; }
}
using App.Utils.Base.Dto;

namespace App.Modules.UserRole.Dto.Responses;

public class UserRoleResponseDto: BaseDto
{
    public required long RoleId { get; set; }
    public required long UserId { get; set; }
    public long? AssignedByUserId { get; set; } 
    public bool Active { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
}
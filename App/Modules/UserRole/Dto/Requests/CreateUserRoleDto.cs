namespace App.Modules.UserRole.Dto.Requests;

public record CreateUserRoleDto(
    long RoleId, 
    long UserId,
    bool Active,
    long? AssignedByUserId
);

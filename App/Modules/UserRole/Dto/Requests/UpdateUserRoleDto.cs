namespace App.Modules.UserRole.Dto.Requests;

public record UpdateUserRoleDto(
    bool? Active,
    long? AssignedByUserId,
    DateTimeOffset? RevokedAt
);

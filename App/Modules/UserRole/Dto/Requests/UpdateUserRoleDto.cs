namespace App.Modules.UserRole.Dto.Requests;

public record UpdateUserRoleDto(
    bool? Active,
    DateTimeOffset? RevokedAt
);

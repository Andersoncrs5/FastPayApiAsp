namespace App.Modules.Role.Dto.Requests;

public record CreateRoleDto(
    string Name,
    string? Description,
    string? NormalizedName,
    bool Active
);
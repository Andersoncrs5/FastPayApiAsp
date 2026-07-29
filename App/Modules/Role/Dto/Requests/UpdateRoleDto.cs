namespace App.Modules.Role.Dto.Requests;

public record UpdateRoleDto(
    string? Name, 
    string? Description,
    bool? Active
);
namespace App.Modules.User.Dto.Requests;

public record UpdateUserDto(
    string? UserName,
    string? Email,
    string? FullName,
    string? Password
    )
{
    
}
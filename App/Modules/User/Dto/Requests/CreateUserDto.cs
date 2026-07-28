namespace App.Modules.User.Dto.Requests;

public record CreateUserDto(
    string UserName,
    string Email,
    string FullName,
    string Password
    )
{
    
}
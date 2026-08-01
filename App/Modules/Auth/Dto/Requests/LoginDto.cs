namespace App.Modules.Auth.Dto.Requests;

public record LoginDto( 
    string Email, 
    string Password
);
namespace App.Modules.Auth.Dto.Responses;

public class TokenResponse()
{
    public required string Token { get; set; }
    public DateTime ExpiresAccessToken { get; set; }
    public required string RefreshToken { get; set; }
    public DateTimeOffset ExpiresRefreshToken { get; set; }
}
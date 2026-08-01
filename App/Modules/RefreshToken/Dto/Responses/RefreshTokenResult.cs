namespace App.Modules.RefreshToken.Dto.Responses;

public sealed record RefreshTokenResult(
    string Token,
    DateTimeOffset ExpiresAt
);
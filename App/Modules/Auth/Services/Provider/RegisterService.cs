using App.Config.Security.Jwt;
using App.Modules.Auth.Dto.Responses;
using App.Modules.Auth.Gateway;
using App.Modules.Auth.Services.Base;
using App.Modules.RefreshToken.Dto.Responses;
using App.Modules.Role.Model;
using App.Modules.User.Dto.Requests;
using App.Modules.User.Model;
using App.Utils.Result;

namespace App.Modules.Auth.Services.Provider;

public class RegisterService(
    AuthGateway gateway
    ): IRegisterService
{
    public async Task<Result<TokenResponse>> Execute(CreateUserDto dto)
    {
        UserEntity user = (await gateway.CreateUserAsync(dto)).UnwrapOrThrow();
        
        var resultRoles = await gateway.GetRoles(user);
        if (resultRoles.IsFailure) return Result<TokenResponse>.Failure(resultRoles.Errors, resultRoles.StatusCode);
        
        List<RoleEntity> roles = resultRoles.Value;

        AccessTokenResponse token = gateway.CreateToken(user, roles);

        RefreshTokenResult refreshToken = await gateway.CreateRefreshToken(user).UnwrapOrThrow();

        var tokens = new TokenResponse
        {
            Token = token.Token,
            ExpiresAccessToken = token.ExpireAt,
            RefreshToken = refreshToken.Token,
            ExpiresRefreshToken = refreshToken.ExpiresAt
        };
        
        return Result<TokenResponse>.Success(tokens);
    }
        
}
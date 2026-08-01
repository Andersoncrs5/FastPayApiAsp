using App.Config.Database;
using App.Config.Options;
using App.Config.uow;
using App.Modules.RefreshToken.Dto.Responses;
using App.Modules.RefreshToken.Gateway;
using App.Modules.RefreshToken.Model;
using App.Modules.RefreshToken.Services.Base;
using App.Utils.Result;
using Microsoft.Extensions.Options;
using Npgsql;

namespace App.Modules.RefreshToken.Services.Provider;

public class CreateRefreshTokenService(
    IOptions<JwtOptions> jwtOptions,
    IUnitOfWork uow,
    RefreshTokenGateway gateway
    ): ICreateRefreshTokenService
{
    public async Task<Result<RefreshTokenResult>> Execute(long userId)
    {
        try
        {
            string token = uow.IdGenerator.CreateId().ToString();

            string tokenHash = gateway.Hash(token);

            DateTimeOffset expiresAt =
                DateTimeOffset.UtcNow.AddMinutes(jwtOptions.Value.RefreshTokenValidityInMinutes);

            RefreshTokenEntity entity = new RefreshTokenEntity
            {
                Id = uow.IdGenerator.CreateId(),
                UserId = userId,
                TokenHash = tokenHash,
                ExpiresAt = expiresAt
            };

            await uow.RefreshTokenRepository.CreateAsync(entity);

            return Result<RefreshTokenResult>.Created(
                new RefreshTokenResult(
                    token,
                    expiresAt));
        }
        catch (PostgresException ex)
        {
            return PostgresResultHandler.Handle<RefreshTokenResult>(ex);
        }
        
    }
}
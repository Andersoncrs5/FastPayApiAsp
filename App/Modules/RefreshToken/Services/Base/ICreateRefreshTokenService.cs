using App.Modules.RefreshToken.Dto.Responses;
using App.Utils.Result;

namespace App.Modules.RefreshToken.Services.Base;

public interface ICreateRefreshTokenService
{
    Task<Result<RefreshTokenResult>> Execute(long userId);
}
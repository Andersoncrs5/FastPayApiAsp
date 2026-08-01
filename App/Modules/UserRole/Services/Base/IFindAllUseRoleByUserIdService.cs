using App.Modules.UserRole.Model;
using App.Utils.Result;

namespace App.Modules.UserRole.Services.Base;

public interface IFindAllUseRoleByUserIdService
{
    Task<Result<List<UserRoleEntity>>> Execute(long userId);
}
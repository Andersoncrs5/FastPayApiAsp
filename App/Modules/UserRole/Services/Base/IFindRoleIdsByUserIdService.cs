using App.Utils.Result;

namespace App.Modules.UserRole.Services.Base;

public interface IFindRoleIdsByUserIdService
{
    Task<Result<List<long>>> Execute(long userId);
}
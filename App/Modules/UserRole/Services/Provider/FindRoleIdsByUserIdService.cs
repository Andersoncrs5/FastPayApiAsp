using App.Config.uow;
using App.Modules.UserRole.Services.Base;
using App.Utils.Result;

namespace App.Modules.UserRole.Services.Provider;

public sealed class FindRoleIdsByUserIdService(
    IUnitOfWork uow) : IFindRoleIdsByUserIdService
{
    public async Task<Result<List<long>>> Execute(long userId)
    {
        List<long> roleIds = await uow.UserRoleRepository.GetRoleIdsByUserIdAsync(userId);

        return Result<List<long>>.Success(roleIds);
    }
}
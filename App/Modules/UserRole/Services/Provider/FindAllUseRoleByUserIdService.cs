using App.Config.uow;
using App.Modules.UserRole.Model;
using App.Modules.UserRole.Services.Base;
using App.Utils.Result;

namespace App.Modules.UserRole.Services.Provider;

public class FindAllUseRoleByUserIdService(IUnitOfWork uow): IFindAllUseRoleByUserIdService
{
    public async Task<Result<List<UserRoleEntity>>> Execute(long userId)
    {
        List<UserRoleEntity> list = await uow.UserRoleRepository.GetAllByUserId(userId);
        
        return Result<List<UserRoleEntity>>.Ok(200, list);
    }
}
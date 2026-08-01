using App.Config.uow;
using App.Modules.Role.Model;
using App.Modules.Role.Services.Base;
using App.Utils.Result;

namespace App.Modules.Role.Services.Provider;

public class FindRolesByIdsService(IUnitOfWork uow)
    : IFindRolesByIdsService
{
    public async Task<Result<List<RoleEntity>>> Execute(List<long> ids)
    {
        if (ids.Count == 0) return Result<List<RoleEntity>>.Success([]);

        List<RoleEntity> roles = await uow.RoleRepository.GetAllByIdsAsync(ids);

        return Result<List<RoleEntity>>.Success(roles);
    }
}
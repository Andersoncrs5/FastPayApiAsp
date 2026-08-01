using App.Config.uow;
using App.Modules.Role.Model;
using App.Modules.Role.Services.Base;
using App.Utils.Result;

namespace App.Modules.Role.Services.Provider;

public class FindRoleByIdService(IUnitOfWork uow)
    : IFindRoleByIdService
{
    public async Task<Result<RoleEntity>> FindByIdAsync(long id)
    {
        RoleEntity? entity = await uow.RoleRepository.GetByIdAsync(id);

        if (entity == null)
            return Result<RoleEntity>.Failure("Role not found", 404);

        return Result<RoleEntity>.Success(entity);
    }
}
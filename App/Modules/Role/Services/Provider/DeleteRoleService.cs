using App.Config.uow;
using App.Modules.Role.Services.Base;
using App.Utils.Result;

namespace App.Modules.Role.Services.Provider;

public class DeleteRoleService(IUnitOfWork uow)
    : IDeleteRoleService
{
    public async Task<Result<object>> DeleteRole(long id)
    {
        int count = await uow.RoleRepository.DeleteAndCountAsync(id);

        if (count == 0)
        {
            return Result<object>.Failure("Role not found", 404);
        }

        return Result<object>.Ok();
    }
}
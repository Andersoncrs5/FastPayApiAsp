using App.Config.uow;
using App.Utils.Result;

namespace App.Modules.UserRole.Services.Provider;

public class DeleteUserRoleService(IUnitOfWork uow)
{
    public async Task<Result<object>> DeleteAsync(long id)
    {
        int count = await uow.UserRoleRepository.DeleteAndCountAsync(id);

        if (count == 0)
            return Result<object>.Failure("User Role not found", 404);

        return Result<object>.Ok();
    }
}
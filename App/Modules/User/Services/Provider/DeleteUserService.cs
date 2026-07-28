using App.Config.uow;
using App.Modules.User.Repositories;
using App.Modules.User.Services.Base;
using App.Utils.Result;

namespace App.Modules.User.Services.Provider;

public class DeleteUserService(IUnitOfWork uow): IDeleteUserService
{
    public async Task<Result<object>> DeleteUser(long id)
    {
        int count = await uow.UserRepository.DeleteAndCountAsync(id);

        if (count == 0)
        {
            return Result<object>.Failure("User not found", 404);
        }
        
        return Result<object>.Ok();
    }
}
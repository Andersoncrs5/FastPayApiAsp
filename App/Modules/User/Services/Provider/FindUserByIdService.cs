using App.Config.uow;
using App.Modules.User.Model;
using App.Modules.User.Services.Base;
using App.Utils.Result;

namespace App.Modules.User.Services.Provider;

public class FindUserByIdService(IUnitOfWork uow): IFindUserByIdService
{
    public async Task<Result<UserEntity>> Execute(long id)
    {
        UserEntity? entity = await uow.UserRepository.GetByIdAsync(id);

        if (entity == null)
        {
            return Result<UserEntity>.Failure("User not found", 404);
        }
        
        return Result<UserEntity>.Success(entity);
    }
        
}
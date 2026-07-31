using App.Modules.User.Model;
using App.Utils.Result;

namespace App.Modules.User.Services.Base;

public interface IFindUserByIdService
{
    Task<Result<UserEntity>> Execute(long id);
    
}
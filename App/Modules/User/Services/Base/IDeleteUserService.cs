using App.Utils.Result;

namespace App.Modules.User.Services.Base;

public interface IDeleteUserService
{
    Task<Result<object>> DeleteUser(long id);
}
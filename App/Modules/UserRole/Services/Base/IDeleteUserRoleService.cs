using App.Utils.Result;

namespace App.Modules.UserRole.Services.Base;

public interface IDeleteUserRoleService
{
    Task<Result<object>> DeleteAsync(long id);
}
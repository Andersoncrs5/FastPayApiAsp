using App.Utils.Result;

namespace App.Modules.Role.Services.Base;

public interface IDeleteRoleService
{
    Task<Result<object>> DeleteRole(long id);
}
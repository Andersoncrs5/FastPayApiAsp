using App.Modules.Role.Model;
using App.Utils.Result;

namespace App.Modules.Role.Services.Base;

public interface IFindRoleByIdService
{
    Task<Result<RoleEntity>> FindByIdAsync(long id);
}
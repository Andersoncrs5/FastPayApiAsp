using App.Modules.Role.Dto.Requests;
using App.Modules.Role.Model;
using App.Utils.Result;

namespace App.Modules.Role.Services.Base;

public interface IUpdateRoleService
{
    Task<Result<RoleEntity>> ExecuteAsync(long id, UpdateRoleDto dto);
}
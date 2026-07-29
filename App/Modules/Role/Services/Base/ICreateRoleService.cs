using App.Modules.Role.Dto.Requests;
using App.Modules.Role.Model;
using App.Utils.Result;

namespace App.Modules.Role.Services.Base;

public interface ICreateRoleService
{
    Task<Result<RoleEntity>> Execute(CreateRoleDto dto);
}
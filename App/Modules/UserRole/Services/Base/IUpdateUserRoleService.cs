using App.Modules.UserRole.Dto.Requests;
using App.Modules.UserRole.Model;
using App.Utils.Result;

namespace App.Modules.UserRole.Services.Base;

public interface IUpdateUserRoleService
{
    Task<Result<UserRoleEntity>> Execute(long userRoleId, UpdateUserRoleDto dto);
}
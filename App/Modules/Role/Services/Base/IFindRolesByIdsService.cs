using App.Modules.Role.Model;
using App.Utils.Result;

namespace App.Modules.Role.Services.Base;

public interface IFindRolesByIdsService
{
    Task<Result<List<RoleEntity>>> Execute(List<long> ids);
}
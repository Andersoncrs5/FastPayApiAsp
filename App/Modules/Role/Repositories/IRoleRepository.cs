using App.Modules.Role.Model;
using App.Utils.Base.Repository;

namespace App.Modules.Role.Repositories;

public interface IRoleRepository: BaseRepository<RoleEntity, long>
{
    Task<bool> ExistsByNameAsync(string name);
    Task<List<RoleEntity>> GetAllByIdsAsync(List<long> ids);
}
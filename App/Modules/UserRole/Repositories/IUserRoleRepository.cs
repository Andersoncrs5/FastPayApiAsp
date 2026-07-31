using App.Modules.UserRole.Model;
using App.Utils.Base.Repository;

namespace App.Modules.UserRole.Repositories;

public interface IUserRoleRepository: BaseRepository<UserRoleEntity, long>
{
    Task<UserRoleEntity?> GetByUserIdAndRoleId(long userId, long roleId);
    Task<List<UserRoleEntity>> GetAllByUserId(long userId, int limit = 20);
    Task<bool> ExistsByUserIdAndRoleId(long userId, long roleId);
}
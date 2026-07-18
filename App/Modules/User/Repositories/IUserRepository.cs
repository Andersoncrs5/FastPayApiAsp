using App.Modules.User.Model;
using App.Utils.Base.Repository;

namespace App.Modules.User.Repositories;

public interface IUserRepository: BaseRepository<UserEntity, long>
{
    
}
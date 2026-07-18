using App.Config.Database;
using App.Modules.User.Model;
using App.Utils.Base.Repository;

namespace App.Modules.User.Repositories;

public class UserRepository(IDatabase database) : BaseRepositoryImpl<UserEntity, long>(database, "users"), IUserRepository
{
    
}
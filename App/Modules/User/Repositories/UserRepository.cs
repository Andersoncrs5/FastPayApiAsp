using App.Config.Database.Session;
using App.Config.Tx;
using App.Modules.User.Model;
using App.Utils.Base.Repository;
using Dapper;

namespace App.Modules.User.Repositories;

public class UserRepository(IDatabaseSession database)
    : BaseRepositoryImpl<UserEntity, long>(database, UserEntity._tableName),
        IUserRepository
{
    public async Task<bool> ExistsByUsernameAsync(string username)
    {
        var sql = $"""
                   SELECT EXISTS (
                       SELECT 1
                       FROM {UserEntity._tableName}
                       WHERE LOWER(user_name) = LOWER(@Username)
                   );
                   """;

        return await Database.Connection.ExecuteScalarAsync<bool>(
            sql,
            new
            {
                Username = username
            },
            Database.Transaction);
    }
    
}
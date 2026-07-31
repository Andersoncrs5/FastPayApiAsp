using System.Text;
using App.Config.Database.Session;
using App.Modules.Role.Model;
using App.Modules.User.Model;
using App.Modules.UserRole.Model;
using App.Utils.Base.Repository;
using Dapper;

namespace App.Modules.UserRole.Repositories;

public class UserRoleRepository(IDatabaseSession database)
    : BaseRepositoryImpl<UserRoleEntity, long>(database, UserRoleEntity._tableName),
        IUserRoleRepository
{
    public async Task<List<UserRoleEntity>> GetAllByUserId(
        long userId,
        int limit = 20)
    {
        const string sql = $"""
                            SELECT *
                            FROM {UserRoleEntity._tableName}
                            WHERE user_id = @UserId
                            LIMIT @Limit;
                            """;

        var list = await Database.Connection.QueryAsync<UserRoleEntity>(
            sql,
            new
            {
                UserId = userId,
                Limit = limit
            },
            Database.Transaction);

        return list.ToList();
    }
    
    public async Task<UserRoleEntity?> GetByUserIdAndRoleId(
        long userId,
        long roleId)
    {
        const string sql = $"""
                            SELECT *
                            FROM {UserRoleEntity._tableName}
                            WHERE user_id = @UserId
                              AND role_id = @RoleId
                            LIMIT 1;
                            """;

        return await Database.Connection.QuerySingleOrDefaultAsync<UserRoleEntity>(
            sql,
            new
            {
                UserId = userId,
                RoleId = roleId
            },
            Database.Transaction);
    }
    
    public async Task<bool> ExistsByUserIdAndRoleId(
        long userId,
        long roleId)
    {
        const string sql = $"""
                           SELECT EXISTS (
                               SELECT 1
                               FROM {UserRoleEntity._tableName}
                               WHERE user_id = @UserId
                                 AND role_id = @RoleId
                                 AND active = TRUE
                           );
                           """;

        return await Database.Connection.ExecuteScalarAsync<bool>(
            sql,
            new
            {
                UserId = userId,
                RoleId = roleId
            },
            Database.Transaction);
    }
    
}
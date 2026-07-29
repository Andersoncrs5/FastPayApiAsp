using App.Config.Database.Session;
using App.Modules.Role.Model;
using App.Utils.Base.Repository;
using Dapper;

namespace App.Modules.Role.Repositories;

public class RoleRepository(IDatabaseSession database)
    : BaseRepositoryImpl<RoleEntity, long>(database, RoleEntity._tableName),
        IRoleRepository
{
    public async Task<bool> ExistsByNameAsync(string name)
    {
        
        var sql = $"""
                   SELECT EXISTS (
                       SELECT 1 
                       FROM {TableName} 
                       WHERE normalized_name = @Name
                   );
                   """;

        return await Database.Connection.ExecuteScalarAsync<bool>(
            sql,
            new
            {
                Name = name.ToUpper()
            },
            Database.Transaction);
    }
}
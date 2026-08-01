using App.Config.Database.Session;
using App.Modules.RefreshToken.Model;
using App.Utils.Base.Repository;
using Dapper;

namespace App.Modules.RefreshToken.Repositories;

public class RefreshTokenRepository(IDatabaseSession database) 
    : BaseRepositoryImpl<RefreshTokenEntity, long>(database, RefreshTokenEntity._tableName),
        IRefreshTokenRepository
{
    public async Task<RefreshTokenEntity?> GetByTokenHashAsync(string tokenHash)
    {
        const string sql = $""" SELECT * FROM {RefreshTokenEntity._tableName} WHERE token_hash = @TokenHash LIMIT 1; """; 
        
        return await Database.Connection.QuerySingleOrDefaultAsync<RefreshTokenEntity>( 
            sql, new { TokenHash = tokenHash }, Database.Transaction);
    }

    public async Task<List<RefreshTokenEntity>> GetAllByUserIdAsync(long userId)
    {
        const string sql = $""" SELECT * FROM {RefreshTokenEntity._tableName} WHERE user_id = @UserId ORDER BY created_at DESC; """;
        
        var tokens = await Database.Connection.QueryAsync<RefreshTokenEntity>( 
            sql, new { UserId = userId }, Database.Transaction); 
        return tokens.ToList();
    }

    public async Task<bool> ExistsByTokenHashAsync(string tokenHash)
    {
        const string sql = $""" SELECT EXISTS ( SELECT 1 FROM {RefreshTokenEntity._tableName} WHERE token_hash = @TokenHash ); """;
        
        return await Database.Connection.ExecuteScalarAsync<bool>( 
            sql, new { TokenHash = tokenHash }, Database.Transaction);
    }
}
using App.Modules.RefreshToken.Model;
using App.Utils.Base.Repository;

namespace App.Modules.RefreshToken.Repositories;

public interface IRefreshTokenRepository: BaseRepository<RefreshTokenEntity, long>
{
    Task<RefreshTokenEntity?> GetByTokenHashAsync(string tokenHash);

    Task<List<RefreshTokenEntity>> GetAllByUserIdAsync(long userId);
    
    Task<bool> ExistsByTokenHashAsync(string tokenHash);
    
}
namespace App.Utils.Base.Repository;

public interface BaseRepository<TEntity, TId>
{
    Task<TEntity?> GetByIdAsync(
        TId id
    );

    Task<int> CountAsync();
    
    Task<bool> ExistsByIdAsync(
        TId id
    );

    Task CreateAsync(
        TEntity entity
    );

    Task UpdateAsync(
        TEntity entity
    );

    Task DeleteAsync(
        TId id
    );
    
    Task<int> DeleteAndCountAsync(
        TId id
    );
    
}
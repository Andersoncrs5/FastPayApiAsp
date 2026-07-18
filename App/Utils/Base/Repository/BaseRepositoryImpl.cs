using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using App.Config.Database;
using Dapper;

namespace App.Utils.Base.Repository;

public abstract class BaseRepositoryImpl<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TEntity, TId>(
    IDatabase database, 
    string tableName) 
    : BaseRepository<TEntity, TId>
{
    protected readonly IDatabase Database = database;
    protected readonly string TableName = tableName;

    private static readonly string[] EntityProperties = typeof(TEntity)
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Select(p => p.Name)
        .ToArray();

    public virtual async Task<TEntity?> GetByIdAsync(TId id)
    {
        var columns = string.Join(", ", EntityProperties);
        var sql = $"SELECT {columns} FROM {TableName} WHERE id = @Id;";

        await using var connection = await Database.OpenConnectionAsync();

        return await connection.QuerySingleOrDefaultAsync<TEntity>(
            sql, 
            new { Id = id });
    }

    public virtual async Task<bool> ExistsByIdAsync(TId id)
    {
        var sql = $"SELECT EXISTS (SELECT 1 FROM {TableName} WHERE id = @Id);";

        await using var connection = await Database.OpenConnectionAsync();

        return await connection.ExecuteScalarAsync<bool>(
            sql, 
            new { Id = id });
    }

    public virtual async Task CreateAsync(TEntity entity)
    {
        string columns = string.Join(", ", EntityProperties);
        string values = string.Join(", ", EntityProperties.Select(p => $"@{p}"));
        var sql = $"INSERT INTO {TableName} ({columns}) VALUES ({values});";

        await using var connection = await Database.OpenConnectionAsync();

        await connection.ExecuteAsync(sql, entity);
    }

    public virtual async Task UpdateAsync(TEntity entity)
    {
        var propertiesToUpdate = EntityProperties.Where(p => !p.Equals("Id", StringComparison.OrdinalIgnoreCase));
        var setClause = string.Join(", ", propertiesToUpdate.Select(p => $"{p} = @{p}"));
        var sql = $"UPDATE {TableName} SET {setClause} WHERE id = @Id;";

        await using var connection = await Database.OpenConnectionAsync();

        await connection.ExecuteAsync(sql, entity);
    }

    public virtual async Task DeleteAsync(TId id)
    {
        var sql = $"DELETE FROM {TableName} WHERE id = @Id;";

        await using var connection = await Database.OpenConnectionAsync();

        await connection.ExecuteAsync(sql, new { Id = id });
    }

    public virtual async Task<long> DeleteAndCountAsync(TId id)
    {
        var sql = $"DELETE FROM {TableName} WHERE id = @Id;";

        await using var connection = await Database.OpenConnectionAsync();

        return await connection.ExecuteAsync(sql, new { Id = id });
    }
}
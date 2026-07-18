using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.RegularExpressions;
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

    private static readonly Dictionary<string, string> PropertyToColumnMap = typeof(TEntity)
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(p => p.CanRead && p.CanWrite)
        .ToDictionary(
            p => p.Name,
            p => ToSnakeCase(p.Name)
        );

    static BaseRepositoryImpl()
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    public virtual async Task<TEntity?> GetByIdAsync(TId id)
    {
        var selectClauses = PropertyToColumnMap.Select(kvp => $"{kvp.Value} AS {kvp.Key}");
        var columns = string.Join(", ", selectClauses);
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
        var columns = string.Join(", ", PropertyToColumnMap.Values);
        var values = string.Join(", ", PropertyToColumnMap.Keys.Select(p => $"@{p}"));
        var sql = $"INSERT INTO {TableName} ({columns}) VALUES ({values});";

        await using var connection = await Database.OpenConnectionAsync();

        await connection.ExecuteAsync(sql, entity);
    }

    public virtual async Task UpdateAsync(TEntity entity)
    {
        var propertiesToUpdate = PropertyToColumnMap.Where(kvp => !kvp.Key.Equals("Id", StringComparison.OrdinalIgnoreCase));
        var setClause = string.Join(", ", propertiesToUpdate.Select(kvp => $"{kvp.Value} = @{kvp.Key}"));
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

    private static string ToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        
        return Regex.Replace(input, "(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z])", "_$1").ToLowerInvariant();
    }
}
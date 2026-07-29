using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.RegularExpressions;
using App.Config.Database.Session;
using App.Config.Tx;
using App.Utils.Base.Entity;
using Dapper;

namespace App.Utils.Base.Repository;

public abstract class BaseRepositoryImpl<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
    TEntity,
    TId>(
    IDatabaseSession database,
    string tableName)
    : BaseRepository<TEntity , TId> where TEntity : BaseEntity
{
    protected readonly IDatabaseSession Database = database;
    protected readonly string TableName = tableName;

    private static readonly Dictionary<string, string> PropertyToColumnMap = typeof(TEntity)
        .GetProperties(BindingFlags.Public | BindingFlags.Instance)
        .Where(p => p.CanRead && p.CanWrite)
        .ToDictionary(
            p => p.Name,
            p => ToSnakeCase(p.Name));

    private static readonly string SelectColumns =
        string.Join(", ", PropertyToColumnMap.Select(kvp => $"{kvp.Value} AS {kvp.Key}"));

    private static readonly string InsertColumns =
        string.Join(", ", PropertyToColumnMap.Values);

    private static readonly string InsertValues =
        string.Join(", ", PropertyToColumnMap.Keys.Select(p => $"@{p}"));

    private static readonly string UpdateClause =
        string.Join(", ",
            PropertyToColumnMap
                .Where(kvp => !kvp.Key.Equals("Id", StringComparison.OrdinalIgnoreCase))
                .Select(kvp => $"{kvp.Value} = @{kvp.Key}"));

    static BaseRepositoryImpl()
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    public virtual Task<TEntity?> GetByIdAsync(TId id)
    {
        var sql = $"""
            SELECT {SelectColumns}
            FROM {TableName}
            WHERE id = @Id;
            """;

        return Database.Connection.QuerySingleOrDefaultAsync<TEntity>(
            sql,
            new { Id = id },
            Database.Transaction);
    }

    public virtual Task<bool> ExistsByIdAsync(TId id)
    {
        var sql = $"""
            SELECT EXISTS (
                SELECT 1
                FROM {TableName}
                WHERE id = @Id
            );
            """;

        return Database.Connection.ExecuteScalarAsync<bool>(
            sql,
            new { Id = id },
            Database.Transaction);
    }

    public virtual Task CreateAsync(TEntity entity)
    {
        entity.CreatedAt = DateTimeOffset.UtcNow;
        
        var sql = $"""
            INSERT INTO {TableName} ({InsertColumns})
            VALUES ({InsertValues});
            """;

        return Database.Connection.ExecuteAsync(
            sql,
            entity,
            Database.Transaction);
    }

    public virtual Task UpdateAsync(TEntity entity)
    {
        var sql = $"""
            UPDATE {TableName}
            SET {UpdateClause}
            WHERE id = @Id;
            """;

        entity.UpdatedAt = DateTimeOffset.UtcNow;
        return Database.Connection.ExecuteAsync(
            sql,
            entity,
            Database.Transaction);
    }

    public virtual Task DeleteAsync(TId id)
    {
        var sql = $"""
            DELETE FROM {TableName}
            WHERE id = @Id;
            """;

        return Database.Connection.ExecuteAsync(
            sql,
            new { Id = id },
            Database.Transaction);
    }

    public virtual Task<int> DeleteAndCountAsync(TId id)
    {
        var sql = $"""
            DELETE FROM {TableName}
            WHERE id = @Id;
            """;

        return Database.Connection.ExecuteAsync(
            sql,
            new { Id = id },
            Database.Transaction);
    }

    private static string ToSnakeCase(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        return Regex.Replace(
            input,
            "(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z])",
            "_$1").ToLowerInvariant();
    }
}
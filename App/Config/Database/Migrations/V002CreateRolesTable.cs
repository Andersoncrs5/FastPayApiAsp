using App.Modules.Role.Model;
using Dapper;
using Npgsql;

namespace App.Config.Database.Migrations;

public class V002CreateRolesTable : IMigration
{
    public int Version => 2;

    public string Name => "Create Role Table";

    public async Task UpAsync(NpgsqlConnection connection, CancellationToken cancellationToken = default)
    {
        const string table = RoleEntity._tableName;

        const string sql = $"""
                            CREATE TABLE {table}
                            (
                                id                  BIGINT PRIMARY KEY,
                                
                                name                VARCHAR(100) NOT NULL,
                                normalized_name     VARCHAR(100) NOT NULL,
                                
                                description         VARCHAR(255),
                                
                                active              BOOLEAN NOT NULL DEFAULT TRUE,
                                
                                created_at          TIMESTAMPTZ NOT NULL,
                                updated_at          TIMESTAMPTZ
                            );

                            CREATE UNIQUE INDEX ux_roles_name ON {table}(normalized_name);

                            CREATE INDEX ix_roles_active ON {table}(active);

                            CREATE INDEX ix_roles_created_at ON {table}(created_at);
                            """;
        
        await connection.ExecuteAsync(sql);
    }
}
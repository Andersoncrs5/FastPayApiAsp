using App.Modules.UserRole.Model;
using Dapper;
using Npgsql;

namespace App.Config.Database.Migrations;

public sealed class V003CreateUserRoleTable : IMigration
{
    public int Version => 3;

    public string Name => "Create User Role Table";

    public async Task UpAsync(
        NpgsqlConnection connection,
        CancellationToken cancellationToken = default)
    {
        const string table = UserRoleEntity._tableName;
        
        
        const string sql =
            $"""
            CREATE TABLE {table}
            (
                id              BIGINT PRIMARY KEY,

                user_id         BIGINT NOT NULL,
                role_id         BIGINT NOT NULL,

                active          BOOLEAN NOT NULL DEFAULT TRUE,

                assigned_by_user_id     BIGINT,
                revoked_by      BIGINT,

                revoked_at      TIMESTAMPTZ,

                created_at      TIMESTAMPTZ NOT NULL,
                updated_at      TIMESTAMPTZ
            );


            ALTER TABLE {table}
            ADD CONSTRAINT fk_user_roles_user
            FOREIGN KEY(user_id)
            REFERENCES users(id)
            ON DELETE CASCADE;


            ALTER TABLE {table}
            ADD CONSTRAINT fk_user_roles_role
            FOREIGN KEY(role_id)
            REFERENCES roles(id)
            ON DELETE CASCADE;


            CREATE UNIQUE INDEX ux_user_roles_user_role
            ON user_roles(user_id, role_id);


            CREATE INDEX ix_user_roles_user_id
            ON user_roles(user_id);


            CREATE INDEX ix_user_roles_role_id
            ON user_roles(role_id);


            CREATE INDEX ix_user_roles_active
            ON user_roles(active);

            """;

        await connection.ExecuteAsync(sql);
    }
}
using App.Modules.User.Model;
using Dapper;
using Npgsql;

namespace App.Config.Database.Migrations;

public sealed class V001CreateUsersTable : IMigration
{
    public int Version => 1;

    public string Name => "Create Users Table";

    public async Task UpAsync(
        NpgsqlConnection connection,
        CancellationToken cancellationToken = default)
    {
        const string table = UserEntity._tableName;
        const string sql = $"""
                           CREATE TABLE {table}
                           (
                               id                  BIGINT PRIMARY KEY,
                           
                               user_name           VARCHAR(100) NOT NULL,
                               normalized_user_name VARCHAR(100) NOT NULL,
                           
                               email               VARCHAR(255) NOT NULL,
                               normalized_email    VARCHAR(255) NOT NULL,
                           
                               email_confirmed     BOOLEAN NOT NULL DEFAULT FALSE,
                           
                               password_hash       TEXT NOT NULL,
                           
                               security_stamp      TEXT,
                               concurrency_stamp   TEXT,
                           
                               phone_number        VARCHAR(20),
                               phone_number_confirmed BOOLEAN NOT NULL DEFAULT FALSE,
                           
                               two_factor_enabled  BOOLEAN NOT NULL DEFAULT FALSE,
                           
                               lockout_end         TIMESTAMPTZ,
                               lockout_enabled     BOOLEAN NOT NULL DEFAULT TRUE,
                           
                               access_failed_count INTEGER NOT NULL DEFAULT 0,
                           
                               full_name           VARCHAR(255) NOT NULL,
                           
                               active           BOOLEAN NOT NULL DEFAULT TRUE,
                           
                               created_at          TIMESTAMPTZ NOT NULL,
                           
                               updated_at          TIMESTAMPTZ,
                           
                               last_login_at       TIMESTAMPTZ
                           );

                           CREATE UNIQUE INDEX ux_users_username
                           ON users(normalized_user_name);
                           
                           CREATE UNIQUE INDEX ux_users_email
                           ON users(normalized_email);
                           
                           CREATE INDEX ix_users_active
                           ON users(active);
                           
                           CREATE INDEX ix_users_created_at
                           ON users(created_at);

                           """;

        await connection.ExecuteAsync(sql);
    }
}
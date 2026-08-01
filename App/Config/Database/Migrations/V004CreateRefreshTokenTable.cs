using App.Modules.RefreshToken.Model;
using Dapper;
using Npgsql;

namespace App.Config.Database.Migrations;

public sealed class V004CreateRefreshTokenTable : IMigration
{
    public int Version => 4;

    public string Name => "Create Refresh Token Table";

    public async Task UpAsync(
        NpgsqlConnection connection,
        CancellationToken cancellationToken = default)
    {
        const string table = RefreshTokenEntity._tableName;

        const string sql =
            $"""
             CREATE TABLE {table}
             (
                 id              BIGINT PRIMARY KEY,

                 user_id         BIGINT NOT NULL,

                 token_hash      VARCHAR(255) NOT NULL,

                 expires_at      TIMESTAMPTZ NOT NULL,
                 revoked_at      TIMESTAMPTZ,

                 created_at      TIMESTAMPTZ NOT NULL,
                 updated_at      TIMESTAMPTZ
             );


             ALTER TABLE {table}
             ADD CONSTRAINT fk_refresh_tokens_user
             FOREIGN KEY(user_id)
             REFERENCES users(id)
             ON DELETE CASCADE;


             CREATE UNIQUE INDEX ux_refresh_tokens_token_hash
             ON {table}(token_hash);


             CREATE INDEX ix_refresh_tokens_user_id
             ON {table}(user_id);


             CREATE INDEX ix_refresh_tokens_expires_at
             ON {table}(expires_at);

             """;

        await connection.ExecuteAsync(sql);
    }
}
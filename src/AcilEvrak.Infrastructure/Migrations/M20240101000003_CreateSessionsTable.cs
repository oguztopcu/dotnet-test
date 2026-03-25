using FluentMigrator;

namespace AcilEvrak.Infrastructure.Migrations;

[Migration(20240101000003)]
public sealed class M20240101000003_CreateSessionsTable : Migration
{
    public override void Up()
    {
        Execute.Sql("""
            CREATE TABLE sessions (
                id                  BIGSERIAL PRIMARY KEY,
                uuid                UUID NOT NULL,
                user_id             BIGINT NOT NULL REFERENCES users(id),
                device_name         VARCHAR(255),
                ip_address          VARCHAR(45),
                user_agent          TEXT,
                refresh_token_hash  VARCHAR(512) NOT NULL,
                last_used_at        TIMESTAMPTZ,
                expires_at          TIMESTAMPTZ NOT NULL,
                revoked_at          TIMESTAMPTZ,
                created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at          TIMESTAMPTZ,
                version             BIGINT NOT NULL DEFAULT 0
            );

            CREATE UNIQUE INDEX idx_sessions_uuid ON sessions(uuid);
            CREATE INDEX idx_sessions_user_id ON sessions(user_id) WHERE revoked_at IS NULL;
            CREATE INDEX idx_sessions_refresh_token ON sessions(refresh_token_hash) WHERE revoked_at IS NULL;
            """);
    }

    public override void Down()
    {
        Execute.Sql("DROP TABLE IF EXISTS sessions");
    }
}

using FluentMigrator;

namespace AcilEvrak.Infrastructure.Migrations;

[Migration(20240101000002)]
public sealed class M20240101000002_CreateUsersTable : Migration
{
    public override void Up()
    {
        Execute.Sql("""
            CREATE TABLE users (
                id              BIGSERIAL PRIMARY KEY,
                uuid            UUID NOT NULL,
                email           VARCHAR(320) NOT NULL,
                password        VARCHAR(512) NOT NULL,
                first_name      VARCHAR(100) NOT NULL,
                last_name       VARCHAR(100) NOT NULL,
                role            VARCHAR(50) NOT NULL DEFAULT 'User',
                is_active       BOOLEAN NOT NULL DEFAULT TRUE,
                created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at      TIMESTAMPTZ,
                created_by      BIGINT NOT NULL DEFAULT 0,
                updated_by      BIGINT,
                deleted_by      BIGINT,
                deleted_at      TIMESTAMPTZ,
                version         BIGINT NOT NULL DEFAULT 0
            );

            CREATE UNIQUE INDEX idx_users_uuid ON users(uuid);
            CREATE UNIQUE INDEX idx_users_email ON users(email) WHERE deleted_at IS NULL;
            """);
    }

    public override void Down()
    {
        Execute.Sql("DROP TABLE IF EXISTS users");
    }
}

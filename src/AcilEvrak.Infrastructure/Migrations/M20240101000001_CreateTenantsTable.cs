using FluentMigrator;

namespace AcilEvrak.Infrastructure.Migrations;

[Migration(20240101000001)]
public sealed class M20240101000001_CreateTenantsTable : Migration
{
    public override void Up()
    {
        Execute.Sql("""
            CREATE TABLE tenants (
                id          BIGSERIAL PRIMARY KEY,
                uuid        UUID NOT NULL,
                name        VARCHAR(255) NOT NULL,
                slug        VARCHAR(255) NOT NULL,
                is_active   BOOLEAN NOT NULL DEFAULT TRUE,
                created_at  TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at  TIMESTAMPTZ,
                version     BIGINT NOT NULL DEFAULT 0
            );

            CREATE UNIQUE INDEX idx_tenants_uuid ON tenants(uuid);
            CREATE UNIQUE INDEX idx_tenants_slug ON tenants(slug);
            """);
    }

    public override void Down()
    {
        Execute.Sql("DROP TABLE IF EXISTS tenants");
    }
}

using FluentMigrator;

namespace AcilEvrak.Infrastructure.Migrations;

[Migration(20240101000004)]
public sealed class M20240101000004_CreateOutboxTable : Migration
{
    public override void Up()
    {
        Execute.Sql("""
            CREATE TABLE outbox_messages (
                id              BIGSERIAL PRIMARY KEY,
                event_type      VARCHAR(255) NOT NULL,
                payload         JSONB NOT NULL,
                tenant_id       BIGINT NOT NULL,
                correlation_id  VARCHAR(255) NOT NULL,
                occurred_at     TIMESTAMPTZ NOT NULL,
                processed_at    TIMESTAMPTZ,
                error           TEXT,
                retry_count     INT NOT NULL DEFAULT 0,
                created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );

            CREATE INDEX idx_outbox_unprocessed ON outbox_messages(id) WHERE processed_at IS NULL;
            """);
    }

    public override void Down()
    {
        Execute.Sql("DROP TABLE IF EXISTS outbox_messages");
    }
}

using System.Data;
using AcilEvrak.Application.Interfaces;
using AcilEvrak.Domain.Interfaces;
using Dapper;

namespace AcilEvrak.Infrastructure.Outbox;

public sealed class OutboxRepository : IOutboxRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public OutboxRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task AddAsync(string eventType, string payload, long tenantId, string correlationId, DateTime occurredAt,
        IDbConnection connection, IDbTransaction transaction, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO outbox_messages (event_type, payload, tenant_id, correlation_id, occurred_at)
            VALUES (@EventType, @Payload, @TenantId, @CorrelationId, @OccurredAt)
            """;

        await connection.ExecuteAsync(
            new CommandDefinition(sql, new
            {
                EventType = eventType,
                Payload = payload,
                TenantId = tenantId,
                CorrelationId = correlationId,
                OccurredAt = occurredAt
            }, transaction, cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<OutboxMessageDto>> GetUnprocessedAsync(int batchSize, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT id, event_type, payload, tenant_id, correlation_id, occurred_at, retry_count
            FROM outbox_messages
            WHERE processed_at IS NULL AND (error IS NULL OR retry_count < 3)
            ORDER BY id
            LIMIT @BatchSize
            FOR UPDATE SKIP LOCKED
            """;

        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<OutboxRow>(
            new CommandDefinition(sql, new { BatchSize = batchSize }, cancellationToken: cancellationToken));

        return rows.Select(r => new OutboxMessageDto(
            r.Id, r.EventType, r.Payload, r.TenantId,
            r.CorrelationId, r.OccurredAt, r.RetryCount)).ToList().AsReadOnly();
    }

    public async Task MarkAsProcessedAsync(long id, CancellationToken cancellationToken = default)
    {
        const string sql = "UPDATE outbox_messages SET processed_at = @ProcessedAt WHERE id = @Id";

        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(
            new CommandDefinition(sql, new { ProcessedAt = DateTime.UtcNow, Id = id }, cancellationToken: cancellationToken));
    }

    public async Task MarkAsFailedAsync(long id, string error, CancellationToken cancellationToken = default)
    {
        const string sql = "UPDATE outbox_messages SET error = @Error, retry_count = retry_count + 1 WHERE id = @Id";

        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        await connection.ExecuteAsync(
            new CommandDefinition(sql, new { Error = error, Id = id }, cancellationToken: cancellationToken));
    }

    private sealed record OutboxRow(
        long Id, string EventType, string Payload, long TenantId,
        string CorrelationId, DateTime OccurredAt, int RetryCount);
}

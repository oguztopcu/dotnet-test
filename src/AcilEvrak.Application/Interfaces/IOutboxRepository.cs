using System.Data;

namespace AcilEvrak.Application.Interfaces;

public interface IOutboxRepository
{
    Task AddAsync(string eventType, string payload, long tenantId, string correlationId, DateTime occurredAt, IDbConnection connection, IDbTransaction transaction, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OutboxMessageDto>> GetUnprocessedAsync(int batchSize, CancellationToken cancellationToken = default);
    Task MarkAsProcessedAsync(long id, CancellationToken cancellationToken = default);
    Task MarkAsFailedAsync(long id, string error, CancellationToken cancellationToken = default);
}

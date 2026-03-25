namespace AcilEvrak.Application.Interfaces;

public sealed record OutboxMessageDto(long Id, string EventType, string Payload, long TenantId, string CorrelationId, DateTime OccurredAt, int RetryCount);

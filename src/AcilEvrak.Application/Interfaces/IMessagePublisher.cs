namespace AcilEvrak.Application.Interfaces;

public interface IMessagePublisher
{
    Task PublishAsync(string eventType, string payload, string correlationId, long tenantId, CancellationToken cancellationToken = default);
}

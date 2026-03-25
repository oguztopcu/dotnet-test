using AcilEvrak.Application.Interfaces;

namespace IntegrationTests.Fixtures;

internal sealed class NoOpMessagePublisher : IMessagePublisher
{
    public Task PublishAsync(string eventType, string payload, string correlationId, long tenantId, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}

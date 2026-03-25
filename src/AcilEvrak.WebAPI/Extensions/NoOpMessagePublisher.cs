using AcilEvrak.Application.Interfaces;

namespace AcilEvrak.WebAPI.Extensions;

internal sealed class NoOpMessagePublisher : IMessagePublisher
{
    public Task PublishAsync(string eventType, string payload, string correlationId, long tenantId, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}

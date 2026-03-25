using AcilEvrak.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AcilEvrak.Infrastructure.Outbox;

public sealed class OutboxBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxBackgroundService> _logger;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(5);
    private const int BatchSize = 20;

    public OutboxBackgroundService(IServiceScopeFactory scopeFactory, ILogger<OutboxBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessagesAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error processing outbox messages");
            }

            await Task.Delay(_pollingInterval, stoppingToken);
        }
    }

    private async Task ProcessOutboxMessagesAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
        var messagePublisher = scope.ServiceProvider.GetRequiredService<IMessagePublisher>();

        var messages = await outboxRepository.GetUnprocessedAsync(BatchSize, cancellationToken);

        foreach (var message in messages)
        {
            try
            {
                await messagePublisher.PublishAsync(
                    message.EventType,
                    message.Payload,
                    message.CorrelationId,
                    message.TenantId,
                    cancellationToken);

                await outboxRepository.MarkAsProcessedAsync(message.Id, cancellationToken);
                _logger.LogInformation("Published outbox message {MessageId} ({EventType})", message.Id, message.EventType);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to publish outbox message {MessageId} ({EventType}), retry {RetryCount}",
                    message.Id, message.EventType, message.RetryCount + 1);

                await outboxRepository.MarkAsFailedAsync(message.Id, ex.Message, cancellationToken);
            }
        }
    }
}

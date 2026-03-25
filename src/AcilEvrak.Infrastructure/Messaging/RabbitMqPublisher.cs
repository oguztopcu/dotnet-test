using System.Text;
using System.Text.Json;
using AcilEvrak.Application.Interfaces;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace AcilEvrak.Infrastructure.Messaging;

public sealed class RabbitMqPublisher : IMessagePublisher
{
    private readonly IConnection _connection;
    private readonly ILogger<RabbitMqPublisher> _logger;

    public RabbitMqPublisher(IConnection connection, ILogger<RabbitMqPublisher> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public Task PublishAsync(string eventType, string payload, string correlationId, long tenantId, CancellationToken cancellationToken = default)
    {
        using var channel = _connection.CreateModel();

        var envelope = new
        {
            eventId = Guid.CreateVersion7(),
            eventType,
            occurredAt = DateTime.UtcNow,
            tenantId,
            correlationId,
            payload = JsonSerializer.Deserialize<object>(payload)
        };

        var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(envelope));

        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.ContentType = "application/json";
        properties.MessageId = Guid.CreateVersion7().ToString();
        properties.CorrelationId = correlationId;
        properties.Headers = new Dictionary<string, object>
        {
            { "tenant_id", tenantId.ToString() }
        };

        channel.BasicPublish(
            exchange: "acilEvrak.exchange",
            routingKey: eventType,
            basicProperties: properties,
            body: body);

        _logger.LogInformation("Published message {EventType} with correlation {CorrelationId}", eventType, correlationId);

        return Task.CompletedTask;
    }
}

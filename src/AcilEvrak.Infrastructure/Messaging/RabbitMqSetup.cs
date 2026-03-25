using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace AcilEvrak.Infrastructure.Messaging;

public sealed class RabbitMqSetup
{
    private readonly IConnection _connection;
    private readonly ILogger<RabbitMqSetup> _logger;

    public RabbitMqSetup(IConnection connection, ILogger<RabbitMqSetup> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public void DeclareTopology()
    {
        using var channel = _connection.CreateModel();

        channel.ExchangeDeclare(
            exchange: "acilEvrak.exchange",
            type: ExchangeType.Topic,
            durable: true,
            autoDelete: false);

        DeclareQueueWithDlq(channel, "acilEvrak.user-created.queue", "users.user.created");
        DeclareQueueWithDlq(channel, "acilEvrak.user-updated.queue", "users.user.updated");

        _logger.LogInformation("RabbitMQ topology declared");
    }

    private static void DeclareQueueWithDlq(IModel channel, string queueName, string routingKey)
    {
        var dlqName = queueName.Replace(".queue", ".dlq");

        channel.QueueDeclare(
            queue: dlqName,
            durable: true,
            exclusive: false,
            autoDelete: false);

        channel.QueueDeclare(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object>
            {
                { "x-dead-letter-exchange", "" },
                { "x-dead-letter-routing-key", dlqName }
            });

        channel.QueueBind(queueName, "acilEvrak.exchange", routingKey);
    }

    public static IConnection CreateConnection(IConfiguration configuration)
    {
        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMQ:Host"] ?? "localhost",
            Port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672"),
            UserName = configuration["RabbitMQ:Username"] ?? "guest",
            Password = configuration["RabbitMQ:Password"] ?? "guest",
            DispatchConsumersAsync = true,
            AutomaticRecoveryEnabled = true
        };

        return factory.CreateConnection();
    }
}

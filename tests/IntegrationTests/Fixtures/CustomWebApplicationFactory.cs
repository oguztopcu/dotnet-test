using AcilEvrak.Application.Interfaces;
using AcilEvrak.Domain.Interfaces;
using AcilEvrak.Infrastructure.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace IntegrationTests.Fixtures;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public CustomWebApplicationFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IDbConnectionFactory));
            if (descriptor is not null)
                services.Remove(descriptor);

            services.AddSingleton<IDbConnectionFactory>(new NpgsqlConnectionFactory(_connectionString));

            var rabbitDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IConnection));
            if (rabbitDescriptor is not null)
                services.Remove(rabbitDescriptor);

            var publisherDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IMessagePublisher));
            if (publisherDescriptor is not null)
                services.Remove(publisherDescriptor);

            services.AddSingleton<IMessagePublisher, NoOpMessagePublisher>();
        });

        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _connectionString,
                ["Jwt:Secret"] = "TestSecretKeyThatIsLongEnoughForHmacSha256Algorithm!!",
                ["Jwt:Issuer"] = "TestIssuer",
                ["Jwt:Audience"] = "TestAudience"
            });
        });
    }
}

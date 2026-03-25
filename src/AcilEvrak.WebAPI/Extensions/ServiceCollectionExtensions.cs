using System.Text;
using AcilEvrak.Application.Interfaces;
using AcilEvrak.Domain.Interfaces;
using AcilEvrak.Infrastructure.Auth;
using AcilEvrak.Infrastructure.Cache;
using AcilEvrak.Infrastructure.Database;
using AcilEvrak.Infrastructure.Messaging;
using AcilEvrak.Infrastructure.Outbox;
using AcilEvrak.Infrastructure.Repositories;
using FluentMigrator.Runner;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using RabbitMQ.Client;

namespace AcilEvrak.WebAPI.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is not configured.");

        services.AddSingleton<IDbConnectionFactory>(new NpgsqlConnectionFactory(connectionString));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ITenantContext, TenantContext>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<ICacheService, InMemoryCacheService>();

        services.AddFluentMigratorCore()
            .ConfigureRunner(runner => runner
                .AddPostgres()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(typeof(AcilEvrak.Infrastructure.Migrations.M20240101000001_CreateTenantsTable).Assembly).For.Migrations());

        return services;
    }

    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        try
        {
            var connection = RabbitMqSetup.CreateConnection(configuration);
            services.AddSingleton(connection);
            services.AddScoped<IMessagePublisher, RabbitMqPublisher>();
            services.AddHostedService<OutboxBackgroundService>();
        }
        catch (Exception)
        {
            services.AddSingleton<IMessagePublisher, NoOpMessagePublisher>();
        }

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(AcilEvrak.Application.Models.Result).Assembly);
        });

        return services;
    }

    public static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration configuration)
    {
        var secret = configuration["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret is not configured.");
        var issuer = configuration["Jwt:Issuer"] ?? "AcilEvrak";
        var audience = configuration["Jwt:Audience"] ?? "AcilEvrak";

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = issuer,
                    ValidAudience = audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();

        return services;
    }

    public static WebApplication RunMigrations(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
        runner.MigrateUp();
        return app;
    }
}

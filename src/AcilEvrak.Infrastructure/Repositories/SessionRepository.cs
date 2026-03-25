using System.Data;
using AcilEvrak.Application.Interfaces;
using AcilEvrak.Domain.Entities;
using AcilEvrak.Domain.Interfaces;
using Dapper;

namespace AcilEvrak.Infrastructure.Repositories;

public sealed class SessionRepository : ISessionRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public SessionRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<(long Id, Guid Uuid)> CreateAsync(Session session, IDbConnection connection, IDbTransaction transaction, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO sessions (uuid, user_id, device_name, ip_address, user_agent, refresh_token_hash, last_used_at, expires_at)
            VALUES (@Uuid, @UserId, @DeviceName, @IpAddress, @UserAgent, @RefreshTokenHash, @LastUsedAt, @ExpiresAt)
            RETURNING id, uuid
            """;

        var row = await connection.QuerySingleAsync<IdUuidRow>(
            new CommandDefinition(sql, new
            {
                session.Uuid,
                session.UserId,
                session.DeviceName,
                session.IpAddress,
                session.UserAgent,
                session.RefreshTokenHash,
                session.LastUsedAt,
                session.ExpiresAt
            }, transaction, cancellationToken: cancellationToken));

        return (row.Id, row.Uuid);
    }

    public async Task RevokeAsync(long id, IDbConnection connection, IDbTransaction transaction, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE sessions
            SET revoked_at = @RevokedAt, updated_at = @UpdatedAt, version = version + 1
            WHERE id = @Id AND revoked_at IS NULL
            """;

        await connection.ExecuteAsync(
            new CommandDefinition(sql, new
            {
                RevokedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Id = id
            }, transaction, cancellationToken: cancellationToken));
    }

    public async Task<Session?> GetByUuidAsync(Guid uuid, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT id, uuid, user_id, device_name, ip_address, user_agent,
                   refresh_token_hash, last_used_at, expires_at, revoked_at, created_at, updated_at, version
            FROM sessions
            WHERE uuid = @Uuid
            """;

        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<SessionRow>(
            new CommandDefinition(sql, new { Uuid = uuid }, cancellationToken: cancellationToken));

        return row is null ? null : MapToSession(row);
    }

    public async Task<Session?> GetActiveByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT id, uuid, user_id, device_name, ip_address, user_agent,
                   refresh_token_hash, last_used_at, expires_at, revoked_at, created_at, updated_at, version
            FROM sessions
            WHERE refresh_token_hash = @RefreshTokenHash
                  AND revoked_at IS NULL AND expires_at > NOW()
            """;

        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<SessionRow>(
            new CommandDefinition(sql, new { RefreshTokenHash = refreshTokenHash }, cancellationToken: cancellationToken));

        return row is null ? null : MapToSession(row);
    }

    public async Task<IReadOnlyList<Session>> GetActiveByUserIdAsync(long userId, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT id, uuid, user_id, device_name, ip_address, user_agent,
                   refresh_token_hash, last_used_at, expires_at, revoked_at, created_at, updated_at, version
            FROM sessions
            WHERE user_id = @UserId
                  AND revoked_at IS NULL AND expires_at > NOW()
            ORDER BY created_at DESC
            """;

        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<SessionRow>(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken));

        return rows.Select(MapToSession).ToList().AsReadOnly();
    }

    private static Session MapToSession(SessionRow row)
    {
        return Session.FromDb(
            row.Id, row.Uuid, row.UserId,
            row.DeviceName, row.IpAddress, row.UserAgent,
            row.RefreshTokenHash, row.LastUsedAt, row.ExpiresAt,
            row.RevokedAt, row.CreatedAt, row.UpdatedAt, row.Version);
    }

    private sealed record IdUuidRow(long Id, Guid Uuid);

    private sealed record SessionRow(
        long Id, Guid Uuid, long UserId,
        string? DeviceName, string? IpAddress, string? UserAgent,
        string RefreshTokenHash, DateTime? LastUsedAt, DateTime ExpiresAt,
        DateTime? RevokedAt, DateTime CreatedAt, DateTime? UpdatedAt, long Version);
}

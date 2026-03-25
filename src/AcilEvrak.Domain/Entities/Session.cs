using AcilEvrak.Domain.Common;

namespace AcilEvrak.Domain.Entities;

public sealed class Session : BaseEntity, IAggregateRoot
{
    public long UserId { get; private set; }
    public string? DeviceName { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public string RefreshTokenHash { get; private set; } = default!;
    public DateTime? LastUsedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }

    private Session() { }

    public static Session Create(long userId, string refreshTokenHash, DateTime expiresAt, string? deviceName, string? ipAddress, string? userAgent)
    {
        return new Session
        {
            UserId = userId,
            RefreshTokenHash = refreshTokenHash,
            ExpiresAt = expiresAt,
            DeviceName = deviceName,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            LastUsedAt = DateTime.UtcNow
        };
    }

    public void Revoke()
    {
        RevokedAt = DateTime.UtcNow;
        MarkUpdated();
    }

    public bool IsValid() => RevokedAt is null && ExpiresAt > DateTime.UtcNow;

    public static Session FromDb(long id, Guid uuid, long userId, string? deviceName, string? ipAddress, string? userAgent, string refreshTokenHash, DateTime? lastUsedAt, DateTime expiresAt, DateTime? revokedAt, DateTime createdAt, DateTime? updatedAt, long version)
    {
        var session = new Session
        {
            UserId = userId,
            DeviceName = deviceName,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            RefreshTokenHash = refreshTokenHash,
            LastUsedAt = lastUsedAt,
            ExpiresAt = expiresAt,
            RevokedAt = revokedAt
        };
        session.SetId(id);
        session.SetUuid(uuid);
        session.SetCreatedAt(createdAt);
        session.SetUpdatedAt(updatedAt);
        session.SetVersion(version);
        return session;
    }
}

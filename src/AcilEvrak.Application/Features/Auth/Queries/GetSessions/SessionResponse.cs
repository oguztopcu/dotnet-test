namespace AcilEvrak.Application.Features.Auth.Queries.GetSessions;

public sealed record SessionResponse(Guid Uuid, string? DeviceName, string? IpAddress, DateTime? LastUsedAt, DateTime ExpiresAt, DateTime CreatedAt);

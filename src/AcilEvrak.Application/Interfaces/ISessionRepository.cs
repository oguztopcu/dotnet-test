using System.Data;
using AcilEvrak.Domain.Entities;

namespace AcilEvrak.Application.Interfaces;

public interface ISessionRepository
{
    Task<(long Id, Guid Uuid)> CreateAsync(Session session, IDbConnection connection, IDbTransaction transaction, CancellationToken cancellationToken = default);
    Task RevokeAsync(long id, IDbConnection connection, IDbTransaction transaction, CancellationToken cancellationToken = default);
    Task<Session?> GetByUuidAsync(Guid uuid, CancellationToken cancellationToken = default);
    Task<Session?> GetActiveByRefreshTokenHashAsync(string refreshTokenHash, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Session>> GetActiveByUserIdAsync(long userId, CancellationToken cancellationToken = default);
}

using System.Data;
using AcilEvrak.Domain.Entities;

namespace AcilEvrak.Application.Interfaces;

public interface IUserRepository
{
    Task<(long Id, Guid Uuid)> CreateAsync(User user, IDbConnection connection, IDbTransaction transaction, CancellationToken cancellationToken = default);
    Task UpdateAsync(User user, IDbConnection connection, IDbTransaction transaction, CancellationToken cancellationToken = default);
    Task SoftDeleteAsync(long id, long deletedBy, IDbConnection connection, IDbTransaction transaction, CancellationToken cancellationToken = default);
    Task<User?> GetByUuidAsync(Guid uuid, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<User> Items, long TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}

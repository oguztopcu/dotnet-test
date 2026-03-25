using System.Data;
using AcilEvrak.Application.Interfaces;
using AcilEvrak.Domain.Entities;
using AcilEvrak.Domain.Interfaces;
using Dapper;

namespace AcilEvrak.Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UserRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<(long Id, Guid Uuid)> CreateAsync(User user, IDbConnection connection, IDbTransaction transaction, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO users (uuid, email, password, first_name, last_name, role, is_active, created_by)
            VALUES (@Uuid, @Email, @Password, @FirstName, @LastName, @Role, @IsActive, @CreatedBy)
            RETURNING id, uuid
            """;

        var row = await connection.QuerySingleAsync<IdUuidRow>(
            new CommandDefinition(sql, new
            {
                user.Uuid,
                Email = user.Email.Value,
                Password = user.PasswordHash.Value,
                user.FirstName,
                user.LastName,
                user.Role,
                user.IsActive,
                user.CreatedBy
            }, transaction, cancellationToken: cancellationToken));

        return (row.Id, row.Uuid);
    }

    public async Task UpdateAsync(User user, IDbConnection connection, IDbTransaction transaction, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE users
            SET first_name = @FirstName, last_name = @LastName, role = @Role, is_active = @IsActive,
                updated_by = @UpdatedBy, updated_at = @UpdatedAt, version = version + 1
            WHERE id = @Id AND deleted_at IS NULL
            """;

        await connection.ExecuteAsync(
            new CommandDefinition(sql, new
            {
                user.FirstName,
                user.LastName,
                user.Role,
                user.IsActive,
                user.UpdatedBy,
                UpdatedAt = DateTime.UtcNow,
                user.Id
            }, transaction, cancellationToken: cancellationToken));
    }

    public async Task SoftDeleteAsync(long id, long deletedBy, IDbConnection connection, IDbTransaction transaction, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE users
            SET deleted_by = @DeletedBy, deleted_at = @DeletedAt, version = version + 1
            WHERE id = @Id AND deleted_at IS NULL
            """;

        await connection.ExecuteAsync(
            new CommandDefinition(sql, new
            {
                DeletedBy = deletedBy,
                DeletedAt = DateTime.UtcNow,
                Id = id
            }, transaction, cancellationToken: cancellationToken));
    }

    public async Task<User?> GetByUuidAsync(Guid uuid, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT id, uuid, email, password, first_name, last_name, role, is_active,
                   created_at, updated_at, created_by, updated_by, deleted_by, deleted_at, version
            FROM users
            WHERE uuid = @Uuid AND deleted_at IS NULL
            """;

        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<UserRow>(
            new CommandDefinition(sql, new { Uuid = uuid }, cancellationToken: cancellationToken));

        return row is null ? null : MapToUser(row);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT id, uuid, email, password, first_name, last_name, role, is_active,
                   created_at, updated_at, created_by, updated_by, deleted_by, deleted_at, version
            FROM users
            WHERE email = @Email AND deleted_at IS NULL
            """;

        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<UserRow>(
            new CommandDefinition(sql, new { Email = email }, cancellationToken: cancellationToken));

        return row is null ? null : MapToUser(row);
    }

    public async Task<User?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT id, uuid, email, password, first_name, last_name, role, is_active,
                   created_at, updated_at, created_by, updated_by, deleted_by, deleted_at, version
            FROM users
            WHERE id = @Id AND deleted_at IS NULL
            """;

        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<UserRow>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));

        return row is null ? null : MapToUser(row);
    }

    public async Task<(IReadOnlyList<User> Items, long TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        const string countSql = "SELECT COUNT(*) FROM users WHERE deleted_at IS NULL";

        const string dataSql = """
            SELECT id, uuid, email, password, first_name, last_name, role, is_active,
                   created_at, updated_at, created_by, updated_by, deleted_by, deleted_at, version
            FROM users
            WHERE deleted_at IS NULL
            ORDER BY id
            LIMIT @PageSize OFFSET @Offset
            """;

        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var totalCount = await connection.ExecuteScalarAsync<long>(
            new CommandDefinition(countSql, cancellationToken: cancellationToken));

        var rows = await connection.QueryAsync<UserRow>(
            new CommandDefinition(dataSql, new
            {
                PageSize = pageSize,
                Offset = (page - 1) * pageSize
            }, cancellationToken: cancellationToken));

        var items = rows.Select(MapToUser).ToList().AsReadOnly();
        return (items, totalCount);
    }

    private static User MapToUser(UserRow row)
    {
        return User.FromDb(
            row.Id, row.Uuid, row.Email, row.Password,
            row.FirstName, row.LastName, row.Role, row.IsActive,
            row.CreatedAt, row.UpdatedAt, row.CreatedBy, row.UpdatedBy,
            row.DeletedBy, row.DeletedAt, row.Version);
    }

    private sealed record IdUuidRow(long Id, Guid Uuid);

    private sealed record UserRow(
        long Id, Guid Uuid, string Email, string Password,
        string FirstName, string LastName, string Role, bool IsActive,
        DateTime CreatedAt, DateTime? UpdatedAt, long CreatedBy, long? UpdatedBy,
        long? DeletedBy, DateTime? DeletedAt, long Version);
}

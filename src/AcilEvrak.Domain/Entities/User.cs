using AcilEvrak.Domain.Common;
using AcilEvrak.Domain.Events.UserEvents;
using AcilEvrak.Domain.ValueObjects;

namespace AcilEvrak.Domain.Entities;

public sealed class User : AuditableEntity, IAggregateRoot
{
    public Email Email { get; private set; } = default!;
    public PasswordHash PasswordHash { get; private set; } = default!;
    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public string Role { get; private set; } = default!;
    public bool IsActive { get; private set; }

    private User() { }

    public static User Create(Email email, PasswordHash passwordHash, string firstName, string lastName, string role, long createdBy)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new Exceptions.ValidationException("INVALID_FIRST_NAME", "First name is required.");
        if (string.IsNullOrWhiteSpace(lastName))
            throw new Exceptions.ValidationException("INVALID_LAST_NAME", "Last name is required.");
        if (string.IsNullOrWhiteSpace(role))
            throw new Exceptions.ValidationException("INVALID_ROLE", "Role is required.");

        var user = new User
        {
            Email = email,
            PasswordHash = passwordHash,
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Role = role,
            IsActive = true
        };
        user.SetCreatedBy(createdBy);

        user.RaiseDomainEvent(new UserCreatedEvent(user.Uuid, email, firstName, lastName, role));

        return user;
    }

    public void Update(string firstName, string lastName, string role, bool isActive, long updatedBy)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new Exceptions.ValidationException("INVALID_FIRST_NAME", "First name is required.");
        if (string.IsNullOrWhiteSpace(lastName))
            throw new Exceptions.ValidationException("INVALID_LAST_NAME", "Last name is required.");

        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Role = role;
        IsActive = isActive;
        MarkUpdated(updatedBy);

        RaiseDomainEvent(new UserUpdatedEvent(Uuid));
    }

    public bool VerifyPassword(string password, Interfaces.IPasswordHasher hasher)
    {
        return hasher.Verify(password, PasswordHash.Value);
    }

    public static User FromDb(long id, Guid uuid, string email, string passwordHash, string firstName, string lastName, string role, bool isActive, DateTime createdAt, DateTime? updatedAt, long createdBy, long? updatedBy, long? deletedBy, DateTime? deletedAt, long version)
    {
        var user = new User
        {
            Email = Email.FromDb(email),
            PasswordHash = PasswordHash.FromDb(passwordHash),
            FirstName = firstName,
            LastName = lastName,
            Role = role,
            IsActive = isActive
        };
        user.SetId(id);
        user.SetUuid(uuid);
        user.SetCreatedBy(createdBy);
        user.SetUpdatedBy(updatedBy);
        user.SetCreatedAt(createdAt);
        user.SetUpdatedAt(updatedAt);
        user.SetVersion(version);
        return user;
    }
}

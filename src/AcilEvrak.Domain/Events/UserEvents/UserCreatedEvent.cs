namespace AcilEvrak.Domain.Events.UserEvents;

public sealed class UserCreatedEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.CreateVersion7();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string EventType => "users.user.created";

    public Guid UserUuid { get; }
    public string Email { get; }
    public string FirstName { get; }
    public string LastName { get; }
    public string Role { get; }

    public UserCreatedEvent(Guid userUuid, string email, string firstName, string lastName, string role)
    {
        UserUuid = userUuid;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        Role = role;
    }
}

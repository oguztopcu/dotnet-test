namespace AcilEvrak.Domain.Events.UserEvents;

public sealed class UserUpdatedEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.CreateVersion7();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
    public string EventType => "users.user.updated";

    public Guid UserUuid { get; }

    public UserUpdatedEvent(Guid userUuid)
    {
        UserUuid = userUuid;
    }
}

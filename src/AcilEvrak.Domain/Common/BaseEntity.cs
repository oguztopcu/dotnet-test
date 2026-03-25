using AcilEvrak.Domain.Events;

namespace AcilEvrak.Domain.Common;

public abstract class BaseEntity
{
    public long Id { get; private set; }
    public Guid Uuid { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public long Version { get; private set; }

    private readonly List<IDomainEvent> _domainEvents = [];
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected BaseEntity()
    {
        Uuid = Guid.CreateVersion7();
        CreatedAt = DateTime.UtcNow;
        Version = 0;
    }

    protected void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
    public void ClearDomainEvents() => _domainEvents.Clear();

    protected void SetId(long id) => Id = id;
    protected void SetUuid(Guid uuid) => Uuid = uuid;
    protected void SetCreatedAt(DateTime createdAt) => CreatedAt = createdAt;
    protected void SetUpdatedAt(DateTime? updatedAt) => UpdatedAt = updatedAt;
    protected void SetVersion(long version) => Version = version;

    public void MarkUpdated()
    {
        UpdatedAt = DateTime.UtcNow;
        Version++;
    }
}

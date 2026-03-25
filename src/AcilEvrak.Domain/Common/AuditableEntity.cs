namespace AcilEvrak.Domain.Common;

public abstract class AuditableEntity : BaseEntity
{
    public long CreatedBy { get; private set; }
    public long? UpdatedBy { get; private set; }
    public long? DeletedBy { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    protected void SetCreatedBy(long userId) => CreatedBy = userId;
    protected void SetUpdatedBy(long? userId) => UpdatedBy = userId;

    public void MarkUpdated(long userId)
    {
        UpdatedBy = userId;
        MarkUpdated();
    }

    public void MarkDeleted(long userId)
    {
        DeletedBy = userId;
        DeletedAt = DateTime.UtcNow;
        MarkUpdated();
    }
}

namespace AcilEvrak.Domain.Common;

public abstract class TenantEntity : AuditableEntity
{
    public long TenantId { get; private set; }

    protected void SetTenantId(long tenantId) => TenantId = tenantId;
}

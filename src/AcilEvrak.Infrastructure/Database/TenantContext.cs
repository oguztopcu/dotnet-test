using AcilEvrak.Domain.Interfaces;

namespace AcilEvrak.Infrastructure.Database;

public sealed class TenantContext : ITenantContext
{
    public long TenantId { get; private set; }

    public void SetTenantId(long tenantId) => TenantId = tenantId;
}

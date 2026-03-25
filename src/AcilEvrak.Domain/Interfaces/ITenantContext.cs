namespace AcilEvrak.Domain.Interfaces;

public interface ITenantContext
{
    long TenantId { get; }
    void SetTenantId(long tenantId);
}

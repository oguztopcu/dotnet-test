using AcilEvrak.Domain.Common;

namespace AcilEvrak.Domain.Entities;

public sealed class Tenant : BaseEntity
{
    public string Name { get; private set; } = default!;
    public string Slug { get; private set; } = default!;
    public bool IsActive { get; private set; }

    private Tenant() { }

    public static Tenant Create(string name, string slug)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new Exceptions.ValidationException("INVALID_TENANT_NAME", "Tenant name is required.");

        if (string.IsNullOrWhiteSpace(slug))
            throw new Exceptions.ValidationException("INVALID_TENANT_SLUG", "Tenant slug is required.");

        return new Tenant
        {
            Name = name,
            Slug = slug.Trim().ToLowerInvariant(),
            IsActive = true
        };
    }

    public static Tenant FromDb(long id, Guid uuid, string name, string slug, bool isActive, DateTime createdAt, DateTime? updatedAt, long version)
    {
        var tenant = new Tenant { Name = name, Slug = slug, IsActive = isActive };
        tenant.SetId(id);
        tenant.SetUuid(uuid);
        tenant.SetCreatedAt(createdAt);
        tenant.SetUpdatedAt(updatedAt);
        tenant.SetVersion(version);
        return tenant;
    }
}

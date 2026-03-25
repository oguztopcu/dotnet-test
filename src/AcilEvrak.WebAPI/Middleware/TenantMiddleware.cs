using AcilEvrak.Domain.Interfaces;

namespace AcilEvrak.WebAPI.Middleware;

public sealed class TenantMiddleware
{
    private const string HeaderName = "X-Tenant-Id";
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        var hasTenantHeader = context.Request.Headers.TryGetValue(HeaderName, out var tenantIdHeader)
                              && long.TryParse(tenantIdHeader, out var tenantId)
                              && tenantId > 0;

        if (hasTenantHeader)
        {
            tenantContext.SetTenantId(long.Parse(tenantIdHeader!));
        }

        var endpoint = context.GetEndpoint();
        var requiresTenant = endpoint?.Metadata.GetMetadata<RequiresTenantAttribute>() is not null;

        if (requiresTenant && !hasTenantHeader)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                error = new { code = "MISSING_TENANT", message = "X-Tenant-Id header is required.", type = "Validation" },
                correlationId = context.Items["CorrelationId"]?.ToString(),
                timestamp = DateTime.UtcNow
            });
            return;
        }

        await _next(context);
    }
}

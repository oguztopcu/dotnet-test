using AcilEvrak.Infrastructure.Cache;

namespace AcilEvrak.WebAPI.Middleware;

public sealed class IdempotencyMiddleware
{
    private const string HeaderName = "X-Idempotency-Key";
    private static readonly HashSet<string> MutatingMethods = ["POST", "PUT", "PATCH"];
    private readonly RequestDelegate _next;

    public IdempotencyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ICacheService cacheService)
    {
        if (!MutatingMethods.Contains(context.Request.Method))
        {
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(HeaderName, out var idempotencyKey) || string.IsNullOrWhiteSpace(idempotencyKey))
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                error = new { code = "MISSING_IDEMPOTENCY_KEY", message = "X-Idempotency-Key header is required for mutating requests.", type = "Validation" },
                correlationId = context.Items["CorrelationId"]?.ToString(),
                timestamp = DateTime.UtcNow
            });
            return;
        }

        var cacheKey = $"idempotency:{idempotencyKey}";
        var cached = await cacheService.GetAsync<string>(cacheKey);

        if (cached is not null)
        {
            context.Response.StatusCode = StatusCodes.Status200OK;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(cached);
            return;
        }

        await _next(context);

        await cacheService.SetAsync(cacheKey, "processed", TimeSpan.FromHours(24));
    }
}

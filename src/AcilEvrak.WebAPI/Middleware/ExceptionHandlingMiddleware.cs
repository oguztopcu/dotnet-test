using System.Net;
using System.Text.Json;
using AcilEvrak.Domain.Exceptions;
using AcilEvrak.WebAPI.Models;

namespace AcilEvrak.WebAPI.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain exception: {Code} - {Message}", ex.Code, ex.Message);
            await WriteErrorResponseAsync(context, MapStatusCode(ex), ex.Code, ex.Message, ex.Type, ex.Details);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await WriteErrorResponseAsync(context, HttpStatusCode.InternalServerError,
                "INTERNAL_ERROR", "An unexpected error occurred.", "InternalError");
        }
    }

    private static HttpStatusCode MapStatusCode(DomainException exception) => exception switch
    {
        ValidationException => HttpStatusCode.BadRequest,
        UnauthorizedException => HttpStatusCode.Unauthorized,
        NotFoundException => HttpStatusCode.NotFound,
        ConflictException => HttpStatusCode.Conflict,
        _ => HttpStatusCode.InternalServerError
    };

    private static async Task WriteErrorResponseAsync(HttpContext context, HttpStatusCode statusCode,
        string code, string message, string type, object? details = null)
    {
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? Guid.CreateVersion7().ToString();
        var response = ApiResponse.Fail(code, message, type, correlationId, details);

        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }
}

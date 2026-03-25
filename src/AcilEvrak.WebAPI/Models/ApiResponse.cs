namespace AcilEvrak.WebAPI.Models;

public sealed class ApiResponse
{
    public bool Success { get; init; }
    public object? Data { get; init; }
    public ApiError? Error { get; init; }
    public string CorrelationId { get; init; } = default!;
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    public static ApiResponse Ok(object? data, string correlationId) => new()
    {
        Success = true,
        Data = data,
        CorrelationId = correlationId
    };

    public static ApiResponse Fail(string code, string message, string type, string correlationId, object? details = null) => new()
    {
        Success = false,
        Error = new ApiError
        {
            Code = code,
            Message = message,
            Type = type,
            Details = details
        },
        CorrelationId = correlationId
    };
}

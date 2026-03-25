namespace AcilEvrak.WebAPI.Models;

public sealed class ApiError
{
    public string Code { get; init; } = default!;
    public string Message { get; init; } = default!;
    public string Type { get; init; } = default!;
    public object? Details { get; init; }
}

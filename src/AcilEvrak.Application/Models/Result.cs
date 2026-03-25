namespace AcilEvrak.Application.Models;

public sealed class Result
{
    public bool IsSuccess { get; }
    public string? ErrorCode { get; }
    public string? ErrorMessage { get; }
    public string? ErrorType { get; }
    public object? ErrorDetails { get; }

    private Result(bool isSuccess, string? errorCode, string? errorMessage, string? errorType, object? errorDetails)
    {
        IsSuccess = isSuccess;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
        ErrorType = errorType;
        ErrorDetails = errorDetails;
    }

    public static Result Success() => new(true, null, null, null, null);

    public static Result Failure(string errorCode, string errorMessage, string errorType, object? details = null)
        => new(false, errorCode, errorMessage, errorType, details);
}

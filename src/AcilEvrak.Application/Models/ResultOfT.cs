namespace AcilEvrak.Application.Models;

public sealed class Result<T>
{
    public bool IsSuccess { get; }
    public T? Data { get; }
    public string? ErrorCode { get; }
    public string? ErrorMessage { get; }
    public string? ErrorType { get; }
    public object? ErrorDetails { get; }

    private Result(bool isSuccess, T? data, string? errorCode, string? errorMessage, string? errorType, object? errorDetails)
    {
        IsSuccess = isSuccess;
        Data = data;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
        ErrorType = errorType;
        ErrorDetails = errorDetails;
    }

    public static Result<T> Success(T data) => new(true, data, null, null, null, null);

    public static Result<T> Failure(string errorCode, string errorMessage, string errorType, object? details = null)
        => new(false, default, errorCode, errorMessage, errorType, details);
}

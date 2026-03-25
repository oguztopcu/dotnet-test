namespace AcilEvrak.Domain.Exceptions;

public sealed class ValidationException : DomainException
{
    public ValidationException(string code, string message, object? details = null)
        : base(code, message, "Validation", details) { }
}

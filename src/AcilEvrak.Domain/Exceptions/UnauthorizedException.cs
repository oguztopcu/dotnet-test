namespace AcilEvrak.Domain.Exceptions;

public sealed class UnauthorizedException : DomainException
{
    public UnauthorizedException(string code, string message, object? details = null)
        : base(code, message, "Unauthorized", details) { }
}

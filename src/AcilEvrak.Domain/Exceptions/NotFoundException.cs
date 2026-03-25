namespace AcilEvrak.Domain.Exceptions;

public sealed class NotFoundException : DomainException
{
    public NotFoundException(string code, string message, object? details = null)
        : base(code, message, "NotFound", details) { }
}

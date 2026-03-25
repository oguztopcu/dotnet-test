namespace AcilEvrak.Domain.Exceptions;

public class DomainException : Exception
{
    public string Code { get; }
    public string Type { get; }
    public object? Details { get; }

    public DomainException(string code, string message, string type, object? details = null)
        : base(message)
    {
        Code = code;
        Type = type;
        Details = details;
    }
}

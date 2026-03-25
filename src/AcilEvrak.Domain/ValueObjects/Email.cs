namespace AcilEvrak.Domain.ValueObjects;

public sealed class Email : IEquatable<Email>
{
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new Exceptions.ValidationException("INVALID_EMAIL", "Email is required.");

        value = value.Trim().ToLowerInvariant();

        if (!value.Contains('@') || value.Length > 320)
            throw new Exceptions.ValidationException("INVALID_EMAIL", "Invalid email format.");

        return new Email(value);
    }

    public static Email FromDb(string value) => new(value);

    public bool Equals(Email? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is Email other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;
}

namespace AcilEvrak.Domain.ValueObjects;

public sealed class PasswordHash : IEquatable<PasswordHash>
{
    public string Value { get; }

    private PasswordHash(string value)
    {
        Value = value;
    }

    public static PasswordHash FromHash(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            throw new Exceptions.ValidationException("INVALID_PASSWORD_HASH", "Password hash cannot be empty.");

        return new PasswordHash(hash);
    }

    public static PasswordHash FromDb(string value) => new(value);

    public bool Equals(PasswordHash? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is PasswordHash other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => "***";

    public static implicit operator string(PasswordHash hash) => hash.Value;
}

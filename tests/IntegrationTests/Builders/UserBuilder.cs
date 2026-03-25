namespace IntegrationTests.Builders;

public sealed class UserBuilder
{
    public string Email { get; private set; } = $"user-{Guid.CreateVersion7():N}@example.com";
    public string Password { get; private set; } = "Password123!";
    public string FirstName { get; private set; } = "Test";
    public string LastName { get; private set; } = "User";
    public string Role { get; private set; } = "User";

    public UserBuilder WithEmail(string email) { Email = email; return this; }
    public UserBuilder WithPassword(string password) { Password = password; return this; }
    public UserBuilder WithFirstName(string firstName) { FirstName = firstName; return this; }
    public UserBuilder WithLastName(string lastName) { LastName = lastName; return this; }
    public UserBuilder WithRole(string role) { Role = role; return this; }

    public object BuildCreateCommand() => new
    {
        Email,
        Password,
        FirstName,
        LastName,
        Role
    };

    public sealed record CreatedUser(Guid Uuid, string Email, string Password);
}

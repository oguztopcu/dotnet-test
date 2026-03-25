namespace AcilEvrak.WebAPI.Controllers;

public sealed record CreateUserRequest(string Email, string Password, string FirstName, string LastName, string Role);

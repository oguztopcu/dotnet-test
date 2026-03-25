namespace AcilEvrak.WebAPI.Controllers;

public sealed record RegisterRequest(string Email, string Password, string FirstName, string LastName);

namespace AcilEvrak.WebAPI.Controllers;

public sealed record UpdateUserRequest(string FirstName, string LastName, string Role, bool IsActive);

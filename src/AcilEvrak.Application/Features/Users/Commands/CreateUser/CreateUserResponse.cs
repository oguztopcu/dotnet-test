namespace AcilEvrak.Application.Features.Users.Commands.CreateUser;

public sealed record CreateUserResponse(Guid Uuid, string Email, string FirstName, string LastName, string Role);

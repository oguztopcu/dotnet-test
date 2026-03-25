namespace AcilEvrak.Application.Features.Auth.Commands.Register;

public sealed record RegisterResponse(Guid Uuid, string Email, string FirstName, string LastName);

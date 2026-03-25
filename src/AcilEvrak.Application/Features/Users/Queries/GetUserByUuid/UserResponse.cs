namespace AcilEvrak.Application.Features.Users.Queries.GetUserByUuid;

public sealed record UserResponse(Guid Uuid, string Email, string FirstName, string LastName, string Role, bool IsActive, DateTime CreatedAt);

namespace AcilEvrak.Application.Features.Users.Queries.GetUsers;

public sealed record UserListItem(Guid Uuid, string Email, string FirstName, string LastName, string Role, bool IsActive, DateTime CreatedAt);

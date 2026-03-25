using AcilEvrak.Application.Models;
using MediatR;

namespace AcilEvrak.Application.Features.Users.Commands.UpdateUser;

public sealed record UpdateUserCommand(Guid Uuid, string FirstName, string LastName, string Role, bool IsActive) : IRequest<Result>;

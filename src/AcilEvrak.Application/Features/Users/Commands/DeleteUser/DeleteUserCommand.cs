using AcilEvrak.Application.Models;
using MediatR;

namespace AcilEvrak.Application.Features.Users.Commands.DeleteUser;

public sealed record DeleteUserCommand(Guid Uuid) : IRequest<Result>;

using AcilEvrak.Application.Models;
using MediatR;

namespace AcilEvrak.Application.Features.Auth.Commands.Logout;

public sealed record LogoutCommand(Guid SessionUuid) : IRequest<Result>;

using AcilEvrak.Application.Models;
using MediatR;

namespace AcilEvrak.Application.Features.Auth.Commands.RevokeSession;

public sealed record RevokeSessionCommand(Guid SessionUuid) : IRequest<Result>;

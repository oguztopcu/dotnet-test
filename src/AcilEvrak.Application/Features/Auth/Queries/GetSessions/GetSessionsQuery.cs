using AcilEvrak.Application.Models;
using MediatR;

namespace AcilEvrak.Application.Features.Auth.Queries.GetSessions;

public sealed record GetSessionsQuery(long UserId) : IRequest<Result<IReadOnlyList<SessionResponse>>>;

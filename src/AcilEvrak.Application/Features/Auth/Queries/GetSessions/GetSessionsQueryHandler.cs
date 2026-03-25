using AcilEvrak.Application.Interfaces;
using AcilEvrak.Application.Models;
using MediatR;

namespace AcilEvrak.Application.Features.Auth.Queries.GetSessions;

public sealed class GetSessionsQueryHandler : IRequestHandler<GetSessionsQuery, Result<IReadOnlyList<SessionResponse>>>
{
    private readonly ISessionRepository _sessionRepository;

    public GetSessionsQueryHandler(ISessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }

    public async Task<Result<IReadOnlyList<SessionResponse>>> Handle(GetSessionsQuery request, CancellationToken cancellationToken)
    {
        var sessions = await _sessionRepository.GetActiveByUserIdAsync(request.UserId, cancellationToken);
        var response = sessions.Select(s => new SessionResponse(s.Uuid, s.DeviceName, s.IpAddress, s.LastUsedAt, s.ExpiresAt, s.CreatedAt)).ToList().AsReadOnly();
        return Result<IReadOnlyList<SessionResponse>>.Success(response);
    }
}

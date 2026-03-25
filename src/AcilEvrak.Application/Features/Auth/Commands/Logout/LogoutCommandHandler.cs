using AcilEvrak.Application.Interfaces;
using AcilEvrak.Application.Models;
using AcilEvrak.Domain.Exceptions;
using AcilEvrak.Domain.Interfaces;
using MediatR;

namespace AcilEvrak.Application.Features.Auth.Commands.Logout;

public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
{
    private readonly ISessionRepository _sessionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LogoutCommandHandler(ISessionRepository sessionRepository, IUnitOfWork unitOfWork)
    {
        _sessionRepository = sessionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var session = await _sessionRepository.GetByUuidAsync(request.SessionUuid, cancellationToken)
            ?? throw new NotFoundException("SESSION_NOT_FOUND", "Session not found.");

        if (!session.IsValid()) return Result.Success();

        await _unitOfWork.BeginAsync(cancellationToken);
        try
        {
            await _sessionRepository.RevokeAsync(session.Id, _unitOfWork.Connection, _unitOfWork.Transaction, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);
            return Result.Success();
        }
        catch
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }
}

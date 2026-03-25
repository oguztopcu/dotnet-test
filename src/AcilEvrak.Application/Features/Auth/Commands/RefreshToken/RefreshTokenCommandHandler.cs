using AcilEvrak.Application.Interfaces;
using AcilEvrak.Application.Models;
using AcilEvrak.Domain.Entities;
using AcilEvrak.Domain.Exceptions;
using AcilEvrak.Domain.Interfaces;
using MediatR;

namespace AcilEvrak.Application.Features.Auth.Commands.RefreshToken;

public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>
{
    private readonly ISessionRepository _sessionRepository;
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _unitOfWork;

    public RefreshTokenCommandHandler(ISessionRepository sessionRepository, IUserRepository userRepository, IJwtTokenService jwtTokenService, IUnitOfWork unitOfWork)
    {
        _sessionRepository = sessionRepository;
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<RefreshTokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = _jwtTokenService.HashToken(request.RefreshToken);
        var session = await _sessionRepository.GetActiveByRefreshTokenHashAsync(tokenHash, cancellationToken)
            ?? throw new UnauthorizedException("INVALID_REFRESH_TOKEN", "Refresh token is invalid or expired.");

        var user = await _userRepository.GetByIdAsync(session.UserId, cancellationToken)
            ?? throw new UnauthorizedException("USER_NOT_FOUND", "User associated with session not found.");

        if (!user.IsActive)
            throw new UnauthorizedException("USER_INACTIVE", "User account is inactive.");

        var newRefreshToken = _jwtTokenService.GenerateRefreshToken();
        var newRefreshTokenHash = _jwtTokenService.HashToken(newRefreshToken);
        var expiresAt = DateTime.UtcNow.AddDays(7);
        var newSession = Session.Create(user.Id, newRefreshTokenHash, expiresAt, request.DeviceName, request.IpAddress, request.UserAgent);

        await _unitOfWork.BeginAsync(cancellationToken);
        try
        {
            await _sessionRepository.RevokeAsync(session.Id, _unitOfWork.Connection, _unitOfWork.Transaction, cancellationToken);
            var (_, sessionUuid) = await _sessionRepository.CreateAsync(newSession, _unitOfWork.Connection, _unitOfWork.Transaction, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);

            var accessToken = _jwtTokenService.GenerateAccessToken(user.Id, user.Uuid, user.Email, user.Role, sessionUuid);
            return Result<RefreshTokenResponse>.Success(new RefreshTokenResponse(accessToken, newRefreshToken, expiresAt));
        }
        catch
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }
}

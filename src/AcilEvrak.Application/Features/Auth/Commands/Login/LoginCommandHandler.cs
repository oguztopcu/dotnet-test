using AcilEvrak.Application.Interfaces;
using AcilEvrak.Application.Models;
using AcilEvrak.Domain.Entities;
using AcilEvrak.Domain.Exceptions;
using AcilEvrak.Domain.Interfaces;
using MediatR;

namespace AcilEvrak.Application.Features.Auth.Commands.Login;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public LoginCommandHandler(IUserRepository userRepository, ISessionRepository sessionRepository, IJwtTokenService jwtTokenService, IPasswordHasher passwordHasher, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _sessionRepository = sessionRepository;
        _jwtTokenService = jwtTokenService;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user is null || !user.VerifyPassword(request.Password, _passwordHasher))
            throw new UnauthorizedException("INVALID_CREDENTIALS", "Invalid email or password.");

        if (!user.IsActive)
            throw new UnauthorizedException("USER_INACTIVE", "User account is inactive.");

        var refreshToken = _jwtTokenService.GenerateRefreshToken();
        var refreshTokenHash = _jwtTokenService.HashToken(refreshToken);
        var expiresAt = DateTime.UtcNow.AddDays(7);
        var session = Session.Create(user.Id, refreshTokenHash, expiresAt, request.DeviceName, request.IpAddress, request.UserAgent);

        await _unitOfWork.BeginAsync(cancellationToken);
        try
        {
            var (_, sessionUuid) = await _sessionRepository.CreateAsync(session, _unitOfWork.Connection, _unitOfWork.Transaction, cancellationToken);
            await _unitOfWork.CommitAsync(cancellationToken);

            var accessToken = _jwtTokenService.GenerateAccessToken(user.Id, user.Uuid, user.Email, user.Role, sessionUuid);
            return Result<LoginResponse>.Success(new LoginResponse(accessToken, refreshToken, expiresAt));
        }
        catch
        {
            await _unitOfWork.RollbackAsync(cancellationToken);
            throw;
        }
    }
}

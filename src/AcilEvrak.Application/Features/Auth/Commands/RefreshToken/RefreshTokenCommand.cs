using AcilEvrak.Application.Models;
using MediatR;

namespace AcilEvrak.Application.Features.Auth.Commands.RefreshToken;

public sealed record RefreshTokenCommand(string RefreshToken, string? DeviceName, string? IpAddress, string? UserAgent) : IRequest<Result<RefreshTokenResponse>>;

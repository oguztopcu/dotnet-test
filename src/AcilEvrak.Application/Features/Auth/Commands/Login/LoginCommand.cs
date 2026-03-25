using AcilEvrak.Application.Models;
using MediatR;

namespace AcilEvrak.Application.Features.Auth.Commands.Login;

public sealed record LoginCommand(string Email, string Password, string? DeviceName, string? IpAddress, string? UserAgent) : IRequest<Result<LoginResponse>>;

namespace AcilEvrak.Application.Features.Auth.Commands.Login;

public sealed record LoginResponse(string AccessToken, string RefreshToken, DateTime ExpiresAt);

namespace AcilEvrak.Domain.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(long userId, Guid userUuid, string email, string role, Guid sessionUuid);
    string GenerateRefreshToken();
    string HashToken(string token);
}

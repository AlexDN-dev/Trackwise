using Trackwise.Domain.Entities;

namespace Trackwise.Application.Common.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    (string Token, DateTimeOffset ExpiresAt) GenerateRefreshToken();
    string HashRefreshToken(string refreshToken);
}
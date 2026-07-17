using Trackwise.Domain.Entities;

namespace Trackwise.Application.Auth;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByHashedTokenAsync(string hashedToken, CancellationToken cancellationToken = default);
    void Add(RefreshToken refreshToken);
}
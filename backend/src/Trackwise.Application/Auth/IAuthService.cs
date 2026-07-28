using Trackwise.Application.Auth.Dtos;

namespace Trackwise.Application.Auth;

public interface IAuthService
{
    Task RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<AuthResponse?> RefreshAsync(RefreshRequest request, CancellationToken cancellationToken = default);
    Task RevokeAsync(string refreshToken, CancellationToken cancellationToken = default);

}
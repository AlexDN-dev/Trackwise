using Microsoft.Extensions.Logging;
using Trackwise.Application.Auth.Dtos;
using Trackwise.Application.Common.Interfaces;
using Trackwise.Domain.Entities;

namespace Trackwise.Application.Auth.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IUnitOfWork unitOfWork,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var email = Normalize(request.Email);

        if (await _userRepository.EmailExistsAsync(email, cancellationToken))
        {
            _logger.LogWarning("Tentative d'inscription refusée : email deja utilise {Email}", email);
            return;
        }

        var user = new User
        {
            Firstname = request.Firstname.Trim(),
            Lastname = request.Lastname.Trim(),
            Email = email,
            PasswordHash = _passwordHasher.Hash(request.Password)
        };

        _userRepository.Add(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Nouvel utilisateur inscrit {UserId} {Email}", user.Id, email);
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var email = Normalize(request.Email);
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Echec de connexion pour {Email}", email);
            return null;
        }

        var response = IssueTokens(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Connexion reussie {UserId}", user.Id);

        return response;
    }

    public async Task<AuthResponse?> RefreshAsync(RefreshRequest request, CancellationToken cancellationToken = default)
    {
        var hashed = _tokenService.HashRefreshToken(request.RefreshToken);
        var stored = await _refreshTokenRepository.GetByHashedTokenAsync(hashed, cancellationToken);
        
        if (stored is null)
        {
            _logger.LogWarning("Refresh refuse : token inconnu");
            return null;
        }

        if (stored.RevokedAt is not null)
        {
            var actifs = await _refreshTokenRepository.GetActiveByUserAsync(stored.UserId, cancellationToken);
            foreach (var t in actifs)
                t.RevokedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        
            _logger.LogWarning("Réutilisation d'un refresh token revoque pour {UserId} : toute la chaine active revoquée", stored.UserId);
            return null;
        }

        if (stored.ExpiresAt <= DateTimeOffset.UtcNow)
        {
            _logger.LogWarning("Refresh refuse : token expire pour {UserId}", stored.UserId);
            return null;
        }

        stored.RevokedAt = DateTimeOffset.UtcNow;

        var response = IssueTokens(stored.User);
        stored.ReplacedByToken = _tokenService.HashRefreshToken(response.RefreshToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Refresh token renouvele pour {UserId}", stored.UserId);

        return response;
    }

    private AuthResponse IssueTokens(User user)
    {
        var accessToken = _tokenService.GenerateAccessToken(user);
        var (refreshToken, expiresAt) = _tokenService.GenerateRefreshToken();

        _refreshTokenRepository.Add(new RefreshToken
        {
            Token = _tokenService.HashRefreshToken(refreshToken),
            UserId = user.Id,
            ExpiresAt = expiresAt
        });

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };
    }

    public async Task RevokeAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var hashed = _tokenService.HashRefreshToken(refreshToken);
        var stored = await _refreshTokenRepository.GetByHashedTokenAsync(hashed, cancellationToken);
        if (stored is null || stored.RevokedAt is not null) return;

        stored.RevokedAt = DateTimeOffset.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Refresh token révoqué pour {UserId}", stored.UserId);
    }

    private static string Normalize(string email) => email.Trim().ToLowerInvariant();
}

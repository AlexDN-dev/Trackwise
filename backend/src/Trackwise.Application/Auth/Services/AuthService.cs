using Microsoft.Extensions.Logging;
using Trackwise.Application.Auth.Dtos;
using Trackwise.Application.Common.Interfaces;
using Trackwise.Domain.Entities;
using Trackwise.Domain.Enums;

namespace Trackwise.Application.Auth.Services;

public class AuthService : IAuthService
{
    private static readonly TimeSpan ConfirmationLifetime = TimeSpan.FromHours(48);
    private static readonly TimeSpan ResetLifetime = TimeSpan.FromHours(1);

    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IEmailTokenRepository _emailTokenRepository;
    private readonly IEmailSender _emailSender;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IEmailTokenRepository emailTokenRepository,
        IEmailSender emailSender,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IUnitOfWork unitOfWork,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _emailTokenRepository = emailTokenRepository;
        _emailSender = emailSender;
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
            PasswordHash = _passwordHasher.Hash(request.Password),
            AiUsageResetAt = DateTimeOffset.UtcNow.AddDays(30),
            EmailConfirmed = false,
        };

        _userRepository.Add(user);

        var rawToken = CreateEmailToken(user.Id, EmailTokenType.EmailConfirmation, ConfirmationLifetime);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Nouvel utilisateur inscrit {UserId} {Email}", user.Id, email);

        await TrySendAsync(
            () => _emailSender.SendEmailConfirmationAsync(user.Email, user.Firstname, rawToken, cancellationToken),
            "confirmation d'inscription", user.Id);
    }

    public async Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var email = Normalize(request.Email);
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Echec de connexion pour {Email}", email);
            return new LoginResult(null, LoginError.InvalidCredentials);
        }

        if (user.IsBanned)
        {
            _logger.LogWarning("Connexion refusée : user banni {UserId}", user.Id);
            return new LoginResult(null, LoginError.InvalidCredentials);
        }

        if (!user.EmailConfirmed)
        {
            _logger.LogWarning("Connexion refusée : email non confirmé {UserId}", user.Id);
            return new LoginResult(null, LoginError.EmailNotConfirmed);
        }

        var response = IssueTokens(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Connexion reussie {UserId}", user.Id);

        return new LoginResult(response, LoginError.None);
    }

    public async Task<AuthResponse?> ConfirmEmailAsync(string token, CancellationToken cancellationToken = default)
    {
        var hash = _tokenService.HashToken(token);
        var stored = await _emailTokenRepository.GetByHashAsync(hash, EmailTokenType.EmailConfirmation, cancellationToken);

        if (!IsUsable(stored))
        {
            _logger.LogWarning("Confirmation email refusée : token invalide/expiré/déjà utilisé");
            return null;
        }

        stored!.UsedAt = DateTimeOffset.UtcNow;

        var user = stored.User;
        if (!user.EmailConfirmed)
        {
            user.EmailConfirmed = true;
            user.EmailConfirmedAt = DateTimeOffset.UtcNow;
        }

        // Auto-login : on émet les tokens comme un login classique.
        var response = IssueTokens(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Email confirmé + auto-login {UserId}", user.Id);
        return response;
    }

    public async Task ResendConfirmationAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(Normalize(email), cancellationToken);

        // Réponse toujours identique côté controller (anti-énumération).
        if (user is null || user.EmailConfirmed)
            return;

        await InvalidateActiveAsync(user.Id, EmailTokenType.EmailConfirmation, cancellationToken);
        var rawToken = CreateEmailToken(user.Id, EmailTokenType.EmailConfirmation, ConfirmationLifetime);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await TrySendAsync(
            () => _emailSender.SendEmailConfirmationAsync(user.Email, user.Firstname, rawToken, cancellationToken),
            "renvoi confirmation", user.Id);
    }

    public async Task ForgotPasswordAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(Normalize(email), cancellationToken);

        if (user is null)
            return;

        await InvalidateActiveAsync(user.Id, EmailTokenType.PasswordReset, cancellationToken);
        var rawToken = CreateEmailToken(user.Id, EmailTokenType.PasswordReset, ResetLifetime);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await TrySendAsync(
            () => _emailSender.SendPasswordResetAsync(user.Email, user.Firstname, rawToken, cancellationToken),
            "reset mot de passe", user.Id);
    }

    public async Task<bool> ResetPasswordAsync(string token, string newPassword, CancellationToken cancellationToken = default)
    {
        var hash = _tokenService.HashToken(token);
        var stored = await _emailTokenRepository.GetByHashAsync(hash, EmailTokenType.PasswordReset, cancellationToken);

        if (!IsUsable(stored))
        {
            _logger.LogWarning("Reset mot de passe refusé : token invalide/expiré/déjà utilisé");
            return false;
        }

        stored!.UsedAt = DateTimeOffset.UtcNow;

        var user = stored.User;
        user.PasswordHash = _passwordHasher.Hash(newPassword);

        // Réinitialiser le mdp prouve la possession de l'email → on confirme si besoin.
        if (!user.EmailConfirmed)
        {
            user.EmailConfirmed = true;
            user.EmailConfirmedAt = DateTimeOffset.UtcNow;
        }

        // Sécurité : on révoque toutes les sessions actives.
        var activeRefresh = await _refreshTokenRepository.GetActiveByUserAsync(user.Id, cancellationToken);
        foreach (var t in activeRefresh)
            t.RevokedAt = DateTimeOffset.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await TrySendAsync(
            () => _emailSender.SendPasswordChangedAsync(user.Email, user.Firstname, cancellationToken),
            "notification mdp modifié", user.Id);

        _logger.LogInformation("Mot de passe réinitialisé {UserId}", user.Id);
        return true;
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

        if (stored.User.IsBanned)
        {
            stored.RevokedAt = DateTimeOffset.UtcNow;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            _logger.LogWarning("Refresh refuse : user banni {UserId}", stored.UserId);
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

    public async Task RevokeAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var hashed = _tokenService.HashRefreshToken(refreshToken);
        var stored = await _refreshTokenRepository.GetByHashedTokenAsync(hashed, cancellationToken);
        if (stored is null || stored.RevokedAt is not null) return;

        stored.RevokedAt = DateTimeOffset.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Refresh token révoqué pour {UserId}", stored.UserId);
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

    // Crée un EmailToken (hash stocké) et renvoie le token EN CLAIR pour l'email.
    private string CreateEmailToken(Guid userId, EmailTokenType type, TimeSpan lifetime)
    {
        var raw = _tokenService.GenerateSecureToken();
        _emailTokenRepository.Add(new EmailToken
        {
            UserId = userId,
            Type = type,
            TokenHash = _tokenService.HashToken(raw),
            ExpiresAt = DateTimeOffset.UtcNow.Add(lifetime),
        });
        return raw;
    }

    private async Task InvalidateActiveAsync(Guid userId, EmailTokenType type, CancellationToken cancellationToken)
    {
        var actifs = await _emailTokenRepository.GetActiveByUserAsync(userId, type, cancellationToken);
        foreach (var t in actifs)
            t.UsedAt = DateTimeOffset.UtcNow;
    }

    private static bool IsUsable(EmailToken? token)
        => token is not null && token.UsedAt is null && token.ExpiresAt > DateTimeOffset.UtcNow;

    private async Task TrySendAsync(Func<Task> send, string label, Guid userId)
    {
        try
        {
            await send();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Echec envoi email ({Label}) pour {UserId}", label, userId);
        }
    }

    private static string Normalize(string email) => email.Trim().ToLowerInvariant();
}
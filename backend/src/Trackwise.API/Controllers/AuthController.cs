using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Trackwise.Application.Auth;
using Trackwise.Application.Auth.Dtos;

namespace Trackwise.API.Controllers;

[EnableRateLimiting("auth")]
[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private const string RefreshCookieName = "trackwise_refresh";

    private readonly IAuthService _authService;
    private readonly IConfiguration _configuration;

    public AuthController(IAuthService authService, IConfiguration configuration)
    {
        _authService = authService;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        await _authService.RegisterAsync(request, cancellationToken);
        return Accepted();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);

        if (result.Error == LoginError.EmailNotConfirmed)
            return StatusCode(StatusCodes.Status403Forbidden,
                new { code = "EMAIL_NOT_CONFIRMED", message = "Confirme ton adresse email pour te connecter." });

        if (result.Auth is null)
            return Unauthorized(new { message = "Email ou mot de passe incorrect." });

        SetRefreshCookie(result.Auth.RefreshToken);
        return Ok(new { accessToken = result.Auth.AccessToken });
    }

    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(ConfirmEmailRequest request, CancellationToken cancellationToken)
    {
        var response = await _authService.ConfirmEmailAsync(request.Token, cancellationToken);
        if (response is null)
            return BadRequest(new { message = "Lien de confirmation invalide ou expiré." });

        SetRefreshCookie(response.RefreshToken);
        return Ok(new { accessToken = response.AccessToken });
    }

    [HttpPost("resend-confirmation")]
    public async Task<IActionResult> ResendConfirmation(ResendConfirmationRequest request, CancellationToken cancellationToken)
    {
        await _authService.ResendConfirmationAsync(request.Email, cancellationToken);
        return Ok(new { message = "Si un compte non confirmé existe pour cette adresse, un email vient d'être envoyé." });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        await _authService.ForgotPasswordAsync(request.Email, cancellationToken);
        return Ok(new { message = "Si un compte existe pour cette adresse, un email de réinitialisation vient d'être envoyé." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var ok = await _authService.ResetPasswordAsync(request.Token, request.NewPassword, cancellationToken);
        return ok
            ? NoContent()
            : BadRequest(new { message = "Lien de réinitialisation invalide ou expiré." });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies[RefreshCookieName];
        if (string.IsNullOrEmpty(refreshToken))
            return Unauthorized(new { message = "Refresh token absent." });

        var response = await _authService.RefreshAsync(
            new RefreshRequest { RefreshToken = refreshToken }, cancellationToken);

        if (response is null)
        {
            DeleteRefreshCookie();
            return Unauthorized(new { message = "Refresh token invalide ou expiré." });
        }

        SetRefreshCookie(response.RefreshToken);
        return Ok(new { accessToken = response.AccessToken });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies[RefreshCookieName];
        if (!string.IsNullOrEmpty(refreshToken))
            await _authService.RevokeAsync(refreshToken, cancellationToken);

        DeleteRefreshCookie();
        return NoContent();
    }

    private void SetRefreshCookie(string token)
    {
        var days = _configuration.GetValue<int?>("Jwt:RefreshTokenDays") ?? 7;
        Response.Cookies.Append(RefreshCookieName, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/api/auth",
            Expires = DateTimeOffset.UtcNow.AddDays(days),
        });
    }

    private void DeleteRefreshCookie()
    {
        Response.Cookies.Delete(RefreshCookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/api/auth",
        });
    }
}
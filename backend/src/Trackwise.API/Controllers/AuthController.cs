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
        var response = await _authService.LoginAsync(request, cancellationToken);
        if (response is null)
            return Unauthorized(new { message = "Email ou mot de passe incorrect." });

        SetRefreshCookie(response.RefreshToken);
        return Ok(new { accessToken = response.AccessToken });
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
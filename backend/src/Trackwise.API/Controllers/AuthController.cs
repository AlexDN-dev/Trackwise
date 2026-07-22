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
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var response = await _authService.RegisterAsync(request, cancellationToken);
        if (response is null)
        {
            return Conflict(new { message = "Email ou mot de passe incorrect." });
        }

        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var response = await _authService.LoginAsync(request, cancellationToken);
        if (response is null)
            return Unauthorized(new { message = "Email ou mot de passe incorrect." });

        return Ok(response);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshRequest request, CancellationToken cancellationToken)
    {
        var response = await _authService.RefreshAsync(request, cancellationToken);
        if (response is null)
            return Unauthorized(new { message = "Refresh token invalide ou expiré." });

        return Ok(response);
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trackwise.API.Extensions;
using Trackwise.Application.AI;
using Trackwise.Application.AI.Dtos;
using Trackwise.Application.Users;
using Trackwise.Application.Users.Dtos;

namespace Trackwise.API.Controllers;

[ApiController]
[Route("api/users/me")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _service;

    public UsersController(IUserService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<UserResponse>> GetMe(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var response = await _service.GetByIdAsync(userId, cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }

    [HttpPatch]
    public async Task<ActionResult<UserResponse>> UpdateMe(
        [FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var response = await _service.UpdateAsync(userId, request, cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var ok = await _service.ChangePasswordAsync(userId, request, cancellationToken);
        return ok
            ? NoContent()
            : BadRequest(new { message = "Mot de passe actuel incorrect." });
    }
    
    [HttpGet("ai-quota")]
    public async Task<ActionResult<AiQuotaStatus>> GetAiQuota(
        [FromServices] IAiQuotaService quota,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        return Ok(await quota.GetAsync(userId, cancellationToken));
    }
}
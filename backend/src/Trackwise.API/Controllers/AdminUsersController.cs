using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trackwise.API.Extensions;
using Trackwise.Application.Admin;
using Trackwise.Application.Admin.Dtos;
using Trackwise.Domain.Entities;

namespace Trackwise.API.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Policy = "Admin")]
public class AdminUsersController : ControllerBase
{
    private readonly IAdminUserService _service;
    private readonly IAdminAuditService _audit;

    public AdminUsersController(IAdminUserService service, IAdminAuditService audit)
    {
        _service = service;
        _audit = audit;
    }

    [HttpGet]
    public async Task<ActionResult<AdminUsersPage>> List(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.ListAsync(page, size, search, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AdminUserDetail>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var user = await _service.GetAsync(id, cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPost("{id:guid}/ban")]
    public async Task<IActionResult> Ban(Guid id, CancellationToken cancellationToken)
    {
        var adminId = User.GetUserId();
        var ok = await _service.BanAsync(adminId, id, GetClientIp(), cancellationToken);
        return ok ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/unban")]
    public async Task<IActionResult> Unban(Guid id, CancellationToken cancellationToken)
    {
        var adminId = User.GetUserId();
        var ok = await _service.UnbanAsync(adminId, id, GetClientIp(), cancellationToken);
        return ok ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/grant-trial")]
    public async Task<IActionResult> GrantTrial(
        Guid id,
        [FromBody] GrantTrialRequest request,
        CancellationToken cancellationToken)
    {
        var adminId = User.GetUserId();
        var ok = await _service.GrantTrialAsync(adminId, id, request.Days, GetClientIp(), cancellationToken);
        return ok ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/grant-tokens")]
    public async Task<IActionResult> GrantTokens(
        Guid id,
        [FromBody] GrantTokensRequest request,
        CancellationToken cancellationToken)
    {
        var adminId = User.GetUserId();
        var ok = await _service.GrantTokensAsync(adminId, id, request.Count, GetClientIp(), cancellationToken);
        return ok ? NoContent() : NotFound();
    }

    [HttpGet("/api/admin/audit")]
    public async Task<ActionResult<List<AdminAuditLog>>> Audit(
        [FromQuery] int limit = 100,
        CancellationToken cancellationToken = default)
    {
        var logs = await _audit.GetRecentAsync(Math.Clamp(limit, 1, 500), cancellationToken);
        return Ok(logs);
    }

    private string? GetClientIp() =>
        HttpContext.Connection.RemoteIpAddress?.ToString();
}
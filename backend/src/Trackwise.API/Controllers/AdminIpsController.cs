using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trackwise.API.Extensions;
using Trackwise.Application.Admin;
using Trackwise.Domain.Entities;

namespace Trackwise.API.Controllers;

[ApiController]
[Route("api/admin/ips")]
[Authorize(Policy = "Admin")]
public class AdminIpsController : ControllerBase
{
    private readonly IBannedIpService _service;

    public AdminIpsController(IBannedIpService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<List<BannedIp>>> List(CancellationToken cancellationToken)
        => Ok(await _service.ListAsync(cancellationToken));

    [HttpPost("ban")]
    public async Task<IActionResult> Ban([FromBody] BanIpRequest request, CancellationToken cancellationToken)
    {
        var adminId = User.GetUserId();
        var ok = await _service.BanAsync(adminId, request.Ip, request.Reason, GetClientIp(), cancellationToken);
        return ok ? NoContent() : BadRequest();
    }

    [HttpPost("unban")]
    public async Task<IActionResult> Unban([FromBody] UnbanIpRequest request, CancellationToken cancellationToken)
    {
        var adminId = User.GetUserId();
        var ok = await _service.UnbanAsync(adminId, request.Ip, GetClientIp(), cancellationToken);
        return ok ? NoContent() : NotFound();
    }

    private string? GetClientIp() => HttpContext.Connection.RemoteIpAddress?.ToString();

    public class BanIpRequest
    {
        [Required, MaxLength(64)]
        public string Ip { get; set; } = null!;
        [MaxLength(500)]
        public string? Reason { get; set; }
    }

    public class UnbanIpRequest
    {
        [Required, MaxLength(64)]
        public string Ip { get; set; } = null!;
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trackwise.Application.Admin;
using Trackwise.Application.Admin.Dtos;

namespace Trackwise.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Policy = "Admin")]
public class AdminStatsController : ControllerBase
{
    private readonly IAdminStatsService _stats;
    private readonly IAdminLogsService _logs;

    public AdminStatsController(IAdminStatsService stats, IAdminLogsService logs)
    {
        _stats = stats;
        _logs = logs;
    }

    [HttpGet("stats")]
    public async Task<ActionResult<AdminStatsResponse>> GetStats(CancellationToken cancellationToken)
        => Ok(await _stats.GetAsync(cancellationToken));

    [HttpGet("logs")]
    public async Task<ActionResult<LogsResponse>> GetLogs(
        [FromQuery] int lines = 200,
        [FromQuery] string? level = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
        => Ok(await _logs.ReadRecentAsync(lines, level, search, cancellationToken));
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trackwise.API.Extensions;
using Trackwise.Application.Stats;
using Trackwise.Application.Stats.Dtos;

namespace Trackwise.API.Controllers;

[ApiController]
[Route("api/stats")]
[Authorize]
public class StatsController : ControllerBase
{
    private readonly IStatsService _service;

    public StatsController(IStatsService service)
    {
        _service = service;
    }

    [HttpGet("free")]
    public async Task<ActionResult<FreeStatsResponse>> GetFree(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        return Ok(await _service.GetFreeStatsAsync(userId, cancellationToken));
    }
    
    [HttpGet("premium")]
    public async Task<ActionResult<PremiumStatsResponse>> GetPremium(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var result = await _service.GetPremiumStatsAsync(userId, cancellationToken);
        return result is null
            ? StatusCode(StatusCodes.Status403Forbidden, new { message = "Réservé au plan Premium." })
            : Ok(result);
    }
}
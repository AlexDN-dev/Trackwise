using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trackwise.Application.Competences;
using Trackwise.Application.Competences.Dtos;

namespace Trackwise.API.Controllers;

[ApiController]
[Route("api/competences")]
[Authorize]
public class CompetencesController : ControllerBase
{
    private readonly IUserCompetenceService _service;

    public CompetencesController(IUserCompetenceService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<CompetenceResponse>>> Search(
        [FromQuery] string? search,
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
        => Ok(await _service.SearchAsync(search, Math.Clamp(limit, 1, 50), cancellationToken));
}
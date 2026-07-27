using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trackwise.API.Extensions;
using Trackwise.Application.Competences;
using Trackwise.Application.Competences.Dtos;

namespace Trackwise.API.Controllers;

[ApiController]
[Route("api/users/me/competences")]
[Authorize]
public class UserCompetencesController : ControllerBase
{
    private readonly IUserCompetenceService _service;

    public UserCompetencesController(IUserCompetenceService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<CompetenceResponse>>> GetMine(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        return Ok(await _service.GetForUserAsync(userId, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<CompetenceResponse>> Add(
        [FromBody] AddUserCompetenceRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var response = await _service.AddAsync(userId, request, cancellationToken);
        return CreatedAtAction(nameof(GetMine), new { }, response);
    }

    [HttpDelete("{competenceId:guid}")]
    public async Task<IActionResult> Remove(Guid competenceId, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var removed = await _service.RemoveAsync(userId, competenceId, cancellationToken);
        return removed ? NoContent() : NotFound();
    }
}
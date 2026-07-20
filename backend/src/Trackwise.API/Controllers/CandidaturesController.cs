using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trackwise.API.Extensions;
using Trackwise.Application.Candidatures;
using Trackwise.Application.Candidatures.Dtos;

namespace Trackwise.API.Controllers;

[ApiController]
[Route("api/candidatures")]
[Authorize]
public class CandidaturesController : ControllerBase
{
    private readonly ICandidatureService _service;

    public CandidaturesController(ICandidatureService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<CandidatureResponse>>> GetAll(CancellationToken cancellationToken)
        => Ok(await _service.GetAllAsync(User.GetUserId(), cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CandidatureResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var candidature = await _service.GetByIdAsync(id, User.GetUserId(), cancellationToken);
        return candidature is null ? NotFound() : Ok(candidature);
    }

    [HttpPost]
    public async Task<ActionResult<CandidatureResponse>> Create(
        CreateCandidatureRequest request, CancellationToken cancellationToken)
    {
        var created = await _service.CreateAsync(User.GetUserId(), request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CandidatureResponse>> Update(
        Guid id, UpdateCandidatureRequest request, CancellationToken cancellationToken)
    {
        var updated = await _service.UpdateAsync(id, User.GetUserId(), request, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteAsync(id, User.GetUserId(), cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trackwise.API.Extensions;
using Trackwise.Application.Candidatures;
using Trackwise.Application.Candidatures.Dtos;
using Microsoft.AspNetCore.RateLimiting;
using Trackwise.Application.AI;
using Trackwise.Application.AI.Dtos;

namespace Trackwise.API.Controllers;

[ApiController]
[Route("api/candidatures")]
[Authorize]
public class CandidaturesController : ControllerBase
{
    private readonly ICandidatureService _service;
    private readonly IAnalysisService _analysisService;
    private readonly IAiQuotaService _quota;
    
    public CandidaturesController(ICandidatureService service, IAnalysisService analysisService, IAiQuotaService quota)
    {
        _service = service;
        _analysisService = analysisService;
        _quota = quota;
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

    [HttpPost("analyze")]
    public async Task<IActionResult> Analyze([FromBody] AnalyzeRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        var check = await _quota.CheckAndIncrementAsync(userId, cancellationToken);
        if (!check.Allowed)
        {
            return StatusCode(StatusCodes.Status429TooManyRequests, new
            {
                message = "Quota d'analyses IA atteint.",
                quota = check.Status,
            });
        }

        var result = await _analysisService.AnalyzeAsync(request.Text, cancellationToken);
        return Ok(result);
    }
}
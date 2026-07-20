using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trackwise.Application.Entreprises;
using Trackwise.Application.Entreprises.Dtos;

namespace Trackwise.API.Controllers;

[ApiController]
[Route("api/entreprises")]
[Authorize]
public class EntreprisesController : ControllerBase
{
    private readonly IEntrepriseService _service;

    public EntreprisesController(IEntrepriseService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<EntrepriseResponse>>> Search([FromQuery] string? search, CancellationToken cancellationToken
    
    ) => Ok(await _service.SearchAsync(search, cancellationToken));
}
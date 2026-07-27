using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trackwise.API.Extensions;
using Trackwise.Application.Subscriptions;
using Trackwise.Application.Subscriptions.Dtos;

namespace Trackwise.API.Controllers;

[ApiController]
[Route("api/subscription")]
[Authorize]
public class SubscriptionController : ControllerBase
{
    private readonly ISubscriptionService _service;

    public SubscriptionController(ISubscriptionService service)
    {
        _service = service;
    }

    [HttpGet("me")]
    public async Task<ActionResult<SubscriptionResponse>> GetMine(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var response = await _service.GetForUserAsync(userId, cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }

    [HttpPost("checkout")]
    public async Task<ActionResult<CheckoutSessionResponse>> CreateCheckout(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var response = await _service.CreateCheckoutSessionAsync(userId, cancellationToken);
        return response is null ? NotFound() : Ok(response);
    }

    [HttpPost("portal")]
    public async Task<ActionResult<CheckoutSessionResponse>> CreatePortal(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var response = await _service.CreatePortalSessionAsync(userId, cancellationToken);
        return response is null ? BadRequest(new { message = "Aucun abonnement actif" }) : Ok(response);
    }
}
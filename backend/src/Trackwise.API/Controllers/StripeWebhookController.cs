using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Trackwise.Application.Subscriptions;

namespace Trackwise.API.Controllers;

[ApiController]
[Route("api/stripe/webhook")]
[AllowAnonymous]
public class StripeWebhookController : ControllerBase
{
    private readonly ISubscriptionService _service;
    private readonly ILogger<StripeWebhookController> _logger;

    public StripeWebhookController(ISubscriptionService service, ILogger<StripeWebhookController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Handle(CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(HttpContext.Request.Body);
        var payload = await reader.ReadToEndAsync(cancellationToken);
        var signature = Request.Headers["Stripe-Signature"].ToString();

        try
        {
            await _service.HandleWebhookAsync(payload, signature, cancellationToken);
            return Ok();
        }
        catch (StripeException ex)
        {
            _logger.LogWarning(ex, "Webhook Stripe rejeté (signature invalide ?)");
            return BadRequest();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur inattendue lors du traitement webhook Stripe");
            return Ok();
        }
    }
}
using Trackwise.Application.Subscriptions.Dtos;

namespace Trackwise.Application.Subscriptions;

public interface ISubscriptionService
{
    Task<SubscriptionResponse?> GetForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<CheckoutSessionResponse?> CreateCheckoutSessionAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<CheckoutSessionResponse?> CreatePortalSessionAsync(Guid userId, CancellationToken cancellationToken = default);
    Task HandleWebhookAsync(string payload, string signatureHeader, CancellationToken cancellationToken = default);
}
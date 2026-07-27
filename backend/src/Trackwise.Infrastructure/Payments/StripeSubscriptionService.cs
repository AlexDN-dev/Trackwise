using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using Trackwise.Application.Auth;
using Trackwise.Application.Common.Interfaces;
using Trackwise.Application.Subscriptions;
using Trackwise.Application.Subscriptions.Dtos;
using Trackwise.Domain.Entities;
using Trackwise.Domain.Enums;

namespace Trackwise.Infrastructure.Payments;

public class StripeSubscriptionService : ISubscriptionService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly StripeSettings _settings;
    private readonly ILogger<StripeSubscriptionService> _logger;

    public StripeSubscriptionService(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IOptions<StripeSettings> settings,
        ILogger<StripeSubscriptionService> logger)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _settings = settings.Value;
        _logger = logger;

        StripeConfiguration.ApiKey = _settings.SecretKey;
    }

    public async Task<SubscriptionResponse?> GetForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null) return null;

        return new SubscriptionResponse
        {
            Plan = user.PlanType,
            PremiumUntil = user.PremiumUntil,
            IsActive = user.PlanType == PlanType.Premium
                       && user.PremiumUntil.HasValue
                       && user.PremiumUntil > DateTimeOffset.UtcNow
        };
    }

    public async Task<CheckoutSessionResponse?> CreateCheckoutSessionAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null) return null;

        var customerId = await EnsureStripeCustomerAsync(user, cancellationToken);

        var options = new SessionCreateOptions
        {
            Customer = customerId,
            Mode = "subscription",
            SuccessUrl = _settings.SuccessUrl,
            CancelUrl = _settings.CancelUrl,
            LineItems = new List<SessionLineItemOptions>
            {
                new()
                {
                    Price = _settings.PremiumPriceId,
                    Quantity = 1
                }
            },
            AllowPromotionCodes = true,
            ClientReferenceId = user.Id.ToString()
        };

        var session = await new SessionService().CreateAsync(options, cancellationToken: cancellationToken);
        return new CheckoutSessionResponse { Url = session.Url };
    }

    public async Task<CheckoutSessionResponse?> CreatePortalSessionAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null || user.StripeCustomerId is null) return null;

        var options = new Stripe.BillingPortal.SessionCreateOptions
        {
            Customer = user.StripeCustomerId,
            ReturnUrl = _settings.PortalReturnUrl
        };

        var session = await new Stripe.BillingPortal.SessionService().CreateAsync(options, cancellationToken: cancellationToken);
        return new CheckoutSessionResponse { Url = session.Url };
    }

    public async Task HandleWebhookAsync(string payload, string signatureHeader, CancellationToken cancellationToken = default)
    {
        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(payload, signatureHeader, _settings.WebhookSecret);
        }
        catch (StripeException ex)
        {
            _logger.LogWarning(ex, "Signature Stripe invalide");
            throw;
        }

        _logger.LogInformation("Stripe webhook reçu : {Type}", stripeEvent.Type);

        switch (stripeEvent.Type)
        {
            case EventTypes.CustomerSubscriptionCreated:
            case EventTypes.CustomerSubscriptionUpdated:
                await SyncSubscriptionAsync((Subscription)stripeEvent.Data.Object, cancellationToken);
                break;

            case EventTypes.CustomerSubscriptionDeleted:
                await CancelSubscriptionAsync((Subscription)stripeEvent.Data.Object, cancellationToken);
                break;

            case EventTypes.InvoicePaid:
                await ExtendPremiumFromInvoiceAsync((Invoice)stripeEvent.Data.Object, cancellationToken);
                break;

            case EventTypes.InvoicePaymentFailed:
                _logger.LogWarning("Paiement échoué pour customer {CustomerId}",
                    ((Invoice)stripeEvent.Data.Object).CustomerId);
                break;
        }
    }

    private async Task<string> EnsureStripeCustomerAsync(User user, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(user.StripeCustomerId))
            return user.StripeCustomerId;

        var customer = await new CustomerService().CreateAsync(new CustomerCreateOptions
        {
            Email = user.Email,
            Name = $"{user.Firstname} {user.Lastname}".Trim(),
            Metadata = new Dictionary<string, string> { ["userId"] = user.Id.ToString() }
        }, cancellationToken: cancellationToken);

        user.StripeCustomerId = customer.Id;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return customer.Id;
    }

    private async Task SyncSubscriptionAsync(Subscription subscription, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByStripeCustomerIdAsync(subscription.CustomerId, cancellationToken);
        if (user is null)
        {
            _logger.LogWarning("Aucun user trouvé pour customer {CustomerId}", subscription.CustomerId);
            return;
        }

        var isActive = subscription.Status is "active" or "trialing";
        user.PlanType = isActive ? PlanType.Premium : PlanType.Free;
        user.StripeSubscriptionId = subscription.Id;
        user.PremiumUntil = isActive ? GetPeriodEnd(subscription) : null;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task CancelSubscriptionAsync(Subscription subscription, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByStripeCustomerIdAsync(subscription.CustomerId, cancellationToken);
        if (user is null) return;

        user.PlanType = PlanType.Free;
        user.StripeSubscriptionId = null;
        user.PremiumUntil = null;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task ExtendPremiumFromInvoiceAsync(Invoice invoice, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(invoice.CustomerId)) return;

        var user = await _userRepository.GetByStripeCustomerIdAsync(invoice.CustomerId, cancellationToken);
        if (user is null) return;

        // Le webhook subscription.updated met déjà à jour PremiumUntil, on log juste
        _logger.LogInformation("Invoice payée pour user {UserId}, montant {Amount}",
            user.Id, invoice.AmountPaid);
    }
    private static DateTimeOffset? GetPeriodEnd(Subscription subscription)
    {
        var end = subscription.Items?.Data?.FirstOrDefault()?.CurrentPeriodEnd;
        if (!end.HasValue) return null;
        return new DateTimeOffset(DateTime.SpecifyKind(end.Value, DateTimeKind.Utc));
    }
}
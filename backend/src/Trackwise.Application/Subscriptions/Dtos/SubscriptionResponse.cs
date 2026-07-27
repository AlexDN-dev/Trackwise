using Trackwise.Domain.Enums;

namespace Trackwise.Application.Subscriptions.Dtos;

public class SubscriptionResponse
{
    public PlanType Plan { get; set; }
    public DateTimeOffset? PremiumUntil { get; set; }
    public bool IsActive { get; set; }
}
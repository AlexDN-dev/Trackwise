using Trackwise.Domain.Enums;

namespace Trackwise.Application.Admin.Dtos;

public class AdminUserDetail
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string Firstname { get; set; } = null!;
    public string Lastname { get; set; } = null!;
    public UserRole Role { get; set; }
    public PlanType Plan { get; set; }
    public DateTimeOffset? PremiumUntil { get; set; }
    public bool IsBanned { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public int AiUsageCount { get; set; }
    public DateTimeOffset AiUsageResetAt { get; set; }
    public string? StripeCustomerId { get; set; }
    public string? StripeSubscriptionId { get; set; }
}
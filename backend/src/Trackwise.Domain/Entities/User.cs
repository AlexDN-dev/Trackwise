using Trackwise.Domain.Common;
using Trackwise.Domain.Enums;

namespace Trackwise.Domain.Entities;

public class User : BaseEntity
{
    public required string Firstname { get; set; }
    public required string Lastname { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public UserRole Role { get; set; } = UserRole.User;
    public PlanType PlanType { get; set; } = PlanType.Free;
    public string? StripeCustomerId { get; set; }
    public string? StripeSubscriptionId { get; set; }
    public DateTimeOffset? PremiumUntil { get; set; }
    public int AiUsageCount { get; set; }
    public DateTimeOffset AiUsageResetAt { get; set; }

    public ICollection<Candidature> Candidatures { get; set; } = new List<Candidature>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<UserCompetence> UserCompetences { get; set; } = new List<UserCompetence>();
}
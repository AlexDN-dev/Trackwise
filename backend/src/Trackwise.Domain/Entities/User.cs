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

    public ICollection<Candidature> Candidatures { get; set; } = new List<Candidature>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
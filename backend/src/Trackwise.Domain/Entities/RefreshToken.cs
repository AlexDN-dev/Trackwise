using Trackwise.Domain.Common;

namespace Trackwise.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public required string Token { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public string? ReplacedByToken { get; set; }
}
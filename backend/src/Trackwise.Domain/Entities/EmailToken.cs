using Trackwise.Domain.Common;
using Trackwise.Domain.Enums;

namespace Trackwise.Domain.Entities;

public class EmailToken : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public EmailTokenType Type { get; set; }
    
    public required string TokenHash { get; set; }
    
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? UsedAt { get; set; }
}
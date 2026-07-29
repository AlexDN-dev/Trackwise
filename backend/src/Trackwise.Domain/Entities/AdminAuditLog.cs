using Trackwise.Domain.Common;

namespace Trackwise.Domain.Entities;

public class AdminAuditLog : BaseEntity
{
    public required Guid AdminId { get; set; }
    public User Admin { get; set; } = null!;
    
    public Guid? TargetUserId { get; set; }
    public User? TargetUser { get; set; }
    
    public required string Action { get; set; }
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
}
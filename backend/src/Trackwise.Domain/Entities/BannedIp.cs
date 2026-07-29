using Trackwise.Domain.Common;

namespace Trackwise.Domain.Entities;

public class BannedIp : BaseEntity
{
    public required string IpAddress { get; set; }
    public string? Reason { get; set; }
    public required Guid BannedByAdminId { get; set; }
}
using Trackwise.Domain.Entities;

namespace Trackwise.Application.Admin;

public interface IAdminAuditService
{
    Task LogAsync(
        Guid adminId, 
        string action, 
        Guid? targetUserId = null, 
        string? details = null,
        string? ipAddress = null, 
        CancellationToken cancellationToken = default);

    Task<List<AdminAuditLog>> GetRecentAsync(int limite = 100, CancellationToken cancellationToken = default);
}
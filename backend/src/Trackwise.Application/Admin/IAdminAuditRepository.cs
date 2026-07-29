using Trackwise.Domain.Entities;

namespace Trackwise.Application.Admin;

public interface IAdminAuditRepository
{
    void Add(AdminAuditLog entry);
    Task<List<AdminAuditLog>> GetRecentAsync(int limit, CancellationToken cancellationToken = default);
}

using Microsoft.EntityFrameworkCore;
using Trackwise.Application.Admin;
using Trackwise.Domain.Entities;
using Trackwise.Infrastructure.Persistence;

namespace Trackwise.Infrastructure.Repositories;

public class AdminAuditRepository : IAdminAuditRepository
{
    private readonly AppDbContext _db;

    public AdminAuditRepository(AppDbContext db) => _db = db;

    public void Add(AdminAuditLog entry) => _db.AdminAuditLogs.Add(entry);

    public Task<List<AdminAuditLog>> GetRecentAsync(int limit, CancellationToken cancellationToken = default) =>
        _db.AdminAuditLogs
            .OrderByDescending(x => x.CreatedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
}

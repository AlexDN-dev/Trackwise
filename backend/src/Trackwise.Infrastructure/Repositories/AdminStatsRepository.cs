using Microsoft.EntityFrameworkCore;
using Trackwise.Application.Admin;
using Trackwise.Infrastructure.Persistence;

namespace Trackwise.Infrastructure.Repositories;

public class AdminStatsRepository : IAdminStatsRepository
{
    private readonly AppDbContext _db;

    public AdminStatsRepository(AppDbContext db) => _db = db;

    public Task<int> CountUsersAsync(CancellationToken cancellationToken = default) =>
        _db.Users.CountAsync(cancellationToken);

    public Task<int> CountUsersByPlanAsync(int planType, CancellationToken cancellationToken = default) =>
        _db.Users.CountAsync(u => (int)u.PlanType == planType, cancellationToken);

    public Task<int> CountBannedUsersAsync(CancellationToken cancellationToken = default) =>
        _db.Users.CountAsync(u => u.IsBanned, cancellationToken);

    public Task<int> CountAdminsAsync(CancellationToken cancellationToken = default) =>
        _db.Users.CountAsync(u => (int)u.Role == 2, cancellationToken);

    public Task<int> CountCandidaturesAsync(CancellationToken cancellationToken = default) =>
        _db.Candidatures.CountAsync(cancellationToken);

    public Task<int> SumAiUsageAsync(CancellationToken cancellationToken = default) =>
        _db.Users.SumAsync(u => u.AiUsageCount, cancellationToken);

    public Task<int> CountSignupsSinceAsync(DateTimeOffset since, CancellationToken cancellationToken = default) =>
        _db.Users.CountAsync(u => u.CreatedAt >= since, cancellationToken);
}
using Trackwise.Application.Admin.Dtos;
using Trackwise.Domain.Enums;

namespace Trackwise.Application.Admin.Services;

public class AdminStatsService : IAdminStatsService
{
    private readonly IAdminStatsRepository _repository;

    public AdminStatsService(IAdminStatsRepository repository) => _repository = repository;

    public async Task<AdminStatsResponse> GetAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var since30 = now.AddDays(-30);
        var since7 = now.AddDays(-7);

        return new AdminStatsResponse
        {
            TotalUsers = await _repository.CountUsersAsync(cancellationToken),
            FreeUsers = await _repository.CountUsersByPlanAsync((int)PlanType.Free, cancellationToken),
            PremiumUsers = await _repository.CountUsersByPlanAsync((int)PlanType.Premium, cancellationToken),
            BannedUsers = await _repository.CountBannedUsersAsync(cancellationToken),
            Admins = await _repository.CountAdminsAsync(cancellationToken),
            TotalCandidatures = await _repository.CountCandidaturesAsync(cancellationToken),
            TotalAiAnalysesThisMonth = await _repository.SumAiUsageAsync(cancellationToken),
            SignupsLast30Days = await _repository.CountSignupsSinceAsync(since30, cancellationToken),
            SignupsLast7Days = await _repository.CountSignupsSinceAsync(since7, cancellationToken),
        };
    }
}
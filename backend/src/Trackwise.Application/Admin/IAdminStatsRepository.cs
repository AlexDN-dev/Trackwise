namespace Trackwise.Application.Admin;

public interface IAdminStatsRepository
{
    Task<int> CountUsersAsync(CancellationToken cancellationToken = default);
    Task<int> CountUsersByPlanAsync(int planType, CancellationToken cancellationToken = default);
    Task<int> CountBannedUsersAsync(CancellationToken cancellationToken = default);
    Task<int> CountAdminsAsync(CancellationToken cancellationToken = default);
    Task<int> CountCandidaturesAsync(CancellationToken cancellationToken = default);
    Task<int> SumAiUsageAsync(CancellationToken cancellationToken = default);
    Task<int> CountSignupsSinceAsync(DateTimeOffset since, CancellationToken cancellationToken = default);
}
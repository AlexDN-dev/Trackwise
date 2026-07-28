using Trackwise.Application.Stats.Dtos;

namespace Trackwise.Application.Stats;

public interface IStatsService
{
    Task<FreeStatsResponse> GetFreeStatsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<PremiumStatsResponse?> GetPremiumStatsAsync(Guid userId, CancellationToken cancellationToken = default);
}
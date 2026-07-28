using Trackwise.Application.AI.Dtos;

namespace Trackwise.Application.AI;

public interface IAiQuotaService
{
    Task<AiQuotaStatus> GetAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<AiQuotaCheckResult> CheckAndIncrementAsync(Guid userId, CancellationToken cancellationToken = default);
}
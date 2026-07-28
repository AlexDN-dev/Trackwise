using Microsoft.Extensions.Logging;
using Trackwise.Application.AI.Dtos;
using Trackwise.Application.Auth;
using Trackwise.Application.Common.Interfaces;
using Trackwise.Domain.Enums;

namespace Trackwise.Application.AI.Services;

public class AiQuotaService : IAiQuotaService
{
    private const int FreeLimit = 10;
    private const int PremiumLimit = 500;
    private static readonly TimeSpan Window = TimeSpan.FromDays(30);

    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AiQuotaService> _logger;

    public AiQuotaService(IUserRepository userRepository, IUnitOfWork unitOfWork, ILogger<AiQuotaService> logger)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<AiQuotaStatus> GetAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
                   ?? throw new KeyNotFoundException($"User {userId} introuvable");
        
        RollIfExpired(user);
        return BuildStatus(user);
    }

    public async Task<AiQuotaCheckResult> CheckAndIncrementAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken)
                   ?? throw new KeyNotFoundException($"User {userId} introuvable");
        
        RollIfExpired(user);

        var limit = ResolveLimit(user);
        if (user.AiUsageCount >= limit)
        {
            _logger.LogInformation("Quota IA attend pour {UserId} : {Used}/{Limit}",
                userId, user.AiUsageCount, limit);
            return new AiQuotaCheckResult(false, BuildStatus(user));
        }

        user.AiUsageCount++;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new AiQuotaCheckResult(true, BuildStatus(user));
    }
    
    private static void RollIfExpired(Domain.Entities.User user)
    {
        if (user.AiUsageResetAt > DateTimeOffset.UtcNow) return;
        user.AiUsageCount = 0;
        user.AiUsageResetAt = DateTimeOffset.UtcNow + Window;
    }

    private static int ResolveLimit(Domain.Entities.User user)
    {
        var isPremiumActive = user.PlanType == PlanType.Premium
                              && (user.PremiumUntil is null || user.PremiumUntil > DateTimeOffset.UtcNow);
        return isPremiumActive ? PremiumLimit : FreeLimit;
    }

    private static AiQuotaStatus BuildStatus(Domain.Entities.User user) =>
        new(user.AiUsageCount, ResolveLimit(user), user.AiUsageResetAt, user.PlanType);
}
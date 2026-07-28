using Trackwise.Domain.Enums;

namespace Trackwise.Application.AI.Dtos;

public record AiQuotaStatus(
    int Used,
    int Limit,
    DateTimeOffset ResetAt,
    PlanType Plan);
    
public record AiQuotaCheckResult(bool Allowed, AiQuotaStatus Status);
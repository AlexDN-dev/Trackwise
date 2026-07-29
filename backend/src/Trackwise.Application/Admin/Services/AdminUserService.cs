using Microsoft.Extensions.Logging;
using Trackwise.Application.Admin.Dtos;
using Trackwise.Application.Auth;
using Trackwise.Application.Common.Interfaces;
using Trackwise.Domain.Entities;
using Trackwise.Domain.Enums;

namespace Trackwise.Application.Admin.Services;

public class AdminUserService : IAdminUserService
{
    private readonly IUserRepository _users;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IAdminAuditService _audit;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AdminUserService> _logger;

    public AdminUserService(
        IUserRepository users,
        IRefreshTokenRepository refreshTokens,
        IAdminAuditService audit,
        IUnitOfWork unitOfWork,
        ILogger<AdminUserService> logger)
    {
        _users = users;
        _refreshTokens = refreshTokens;
        _audit = audit;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<AdminUsersPage> ListAsync(
        int page, int size, string? search, CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        size = Math.Clamp(size, 1, 100);

        var (items, total) = await _users.GetPagedAsync(page, size, search, cancellationToken);

        return new AdminUsersPage
        {
            Page = page,
            Size = size,
            Total = total,
            Items = items.Select(ToListItem).ToList(),
        };
    }

    public async Task<AdminUserDetail?> GetAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByIdAsync(userId, cancellationToken);
        return user is null ? null : ToDetail(user);
    }

    public async Task<bool> BanAsync(
        Guid adminId, Guid userId, string? ipAddress, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByIdAsync(userId, cancellationToken);
        if (user is null || user.Id == adminId) return false;

        user.IsBanned = true;

        var activeTokens = await _refreshTokens.GetActiveByUserAsync(userId, cancellationToken);
        foreach (var token in activeTokens)
            token.RevokedAt = DateTimeOffset.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(adminId, "BanUser", userId, null, ipAddress, cancellationToken);

        _logger.LogInformation("User {UserId} banni par admin {AdminId}", userId, adminId);
        return true;
    }

    public async Task<bool> UnbanAsync(
        Guid adminId, Guid userId, string? ipAddress, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByIdAsync(userId, cancellationToken);
        if (user is null) return false;

        user.IsBanned = false;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(adminId, "UnbanUser", userId, null, ipAddress, cancellationToken);

        _logger.LogInformation("User {UserId} unban par admin {AdminId}", userId, adminId);
        return true;
    }

    public async Task<bool> GrantTrialAsync(
        Guid adminId, Guid userId, int days, string? ipAddress, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByIdAsync(userId, cancellationToken);
        if (user is null) return false;

        var now = DateTimeOffset.UtcNow;
        var baseDate = user.PremiumUntil.HasValue && user.PremiumUntil > now
            ? user.PremiumUntil.Value
            : now;

        user.PlanType = PlanType.Premium;
        user.PremiumUntil = baseDate.AddDays(days);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(adminId, "GrantTrial", userId, $"{{\"days\":{days}}}", ipAddress, cancellationToken);

        _logger.LogInformation("Trial {Days}j accordé à {UserId} par admin {AdminId}", days, userId, adminId);
        return true;
    }

    public async Task<bool> GrantTokensAsync(
        Guid adminId, Guid userId, int count, string? ipAddress, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByIdAsync(userId, cancellationToken);
        if (user is null) return false;

        user.AiUsageCount = Math.Max(0, user.AiUsageCount - count);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _audit.LogAsync(adminId, "GrantTokens", userId, $"{{\"count\":{count}}}", ipAddress, cancellationToken);

        _logger.LogInformation("Tokens IA +{Count} accordés à {UserId} par admin {AdminId}", count, userId, adminId);
        return true;
    }

    private static AdminUserListItem ToListItem(User u) => new()
    {
        Id = u.Id,
        Email = u.Email,
        Firstname = u.Firstname,
        Lastname = u.Lastname,
        Role = u.Role,
        Plan = u.PlanType,
        PremiumUntil = u.PremiumUntil,
        IsBanned = u.IsBanned,
        CreatedAt = u.CreatedAt,
    };

    private static AdminUserDetail ToDetail(User u) => new()
    {
        Id = u.Id,
        Email = u.Email,
        Firstname = u.Firstname,
        Lastname = u.Lastname,
        Role = u.Role,
        Plan = u.PlanType,
        PremiumUntil = u.PremiumUntil,
        IsBanned = u.IsBanned,
        CreatedAt = u.CreatedAt,
        AiUsageCount = u.AiUsageCount,
        AiUsageResetAt = u.AiUsageResetAt,
        StripeCustomerId = u.StripeCustomerId,
        StripeSubscriptionId = u.StripeSubscriptionId,
    };
}
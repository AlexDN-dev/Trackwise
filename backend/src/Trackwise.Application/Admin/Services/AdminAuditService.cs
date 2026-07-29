using Microsoft.Extensions.Logging;
using Trackwise.Application.Common.Interfaces;
using Trackwise.Domain.Entities;

namespace Trackwise.Application.Admin.Services;

public class AdminAuditService : IAdminAuditService
{
    private readonly IAdminAuditRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AdminAuditService> _logger;

    public AdminAuditService(
        IAdminAuditRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<AdminAuditService> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task LogAsync(
        Guid adminId,
        string action,
        Guid? targetUserId = null,
        string? details = null,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        _repository.Add(new AdminAuditLog
        {
            AdminId = adminId,
            Action = action,
            TargetUserId = targetUserId,
            Details = details,
            IpAddress = ipAddress,
        });
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "[Admin] {Action} par {AdminId} sur {TargetUserId} — {Details}",
            action, adminId, targetUserId, details);
    }

    public Task<List<AdminAuditLog>> GetRecentAsync(int limit = 100, CancellationToken cancellationToken = default) =>
        _repository.GetRecentAsync(limit, cancellationToken);
}

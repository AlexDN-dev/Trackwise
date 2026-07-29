using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Trackwise.Application.Common.Interfaces;
using Trackwise.Domain.Entities;

namespace Trackwise.Application.Admin.Services;

public class BannedIpService : IBannedIpService
{
    private const string CacheKey = "bannedIps";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(30);

    private readonly IBannedIpRepository _repository;
    private readonly IAdminAuditService _audit;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;
    private readonly ILogger<BannedIpService> _logger;

    public BannedIpService(
        IBannedIpRepository repository,
        IAdminAuditService audit,
        IUnitOfWork unitOfWork,
        IMemoryCache cache,
        ILogger<BannedIpService> logger)
    {
        _repository = repository;
        _audit = audit;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
    }

    public Task<List<BannedIp>> ListAsync(CancellationToken cancellationToken = default) =>
        _repository.GetAllAsync(cancellationToken);

    public async Task<bool> IsBannedAsync(string ip, CancellationToken cancellationToken = default)
    {
        var bannedSet = await _cache.GetOrCreateAsync(CacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheTtl;
            var all = await _repository.GetAllAsync(cancellationToken);
            return new HashSet<string>(all.Select(b => b.IpAddress), StringComparer.OrdinalIgnoreCase);
        });

        return bannedSet?.Contains(ip) ?? false;
    }

    public async Task<bool> BanAsync(
        Guid adminId, string ip, string? reason, string? adminIp, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(ip)) return false;

        var existing = await _repository.GetByIpAsync(ip, cancellationToken);
        if (existing is not null) return true;

        _repository.Add(new BannedIp
        {
            IpAddress = ip,
            Reason = reason,
            BannedByAdminId = adminId,
        });
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _cache.Remove(CacheKey);

        await _audit.LogAsync(adminId, "BanIp", null,
            $"{{\"ip\":\"{ip}\",\"reason\":\"{reason}\"}}", adminIp, cancellationToken);

        _logger.LogInformation("IP {Ip} bannie par admin {AdminId}", ip, adminId);
        return true;
    }

    public async Task<bool> UnbanAsync(
        Guid adminId, string ip, string? adminIp, CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIpAsync(ip, cancellationToken);
        if (existing is null) return false;

        _repository.Remove(existing);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        _cache.Remove(CacheKey);

        await _audit.LogAsync(adminId, "UnbanIp", null,
            $"{{\"ip\":\"{ip}\"}}", adminIp, cancellationToken);

        _logger.LogInformation("IP {Ip} unban par admin {AdminId}", ip, adminId);
        return true;
    }
}
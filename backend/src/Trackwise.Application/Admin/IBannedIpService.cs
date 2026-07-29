using Trackwise.Domain.Entities;

namespace Trackwise.Application.Admin;

public interface IBannedIpService
{
    Task<List<BannedIp>> ListAsync(CancellationToken cancellationToken = default);
    Task<bool> IsBannedAsync(string ip, CancellationToken cancellationToken = default);
    Task<bool> BanAsync(Guid adminId, string ip, string? reason, string? adminIp, CancellationToken cancellationToken = default);
    Task<bool> UnbanAsync(Guid adminId, string ip, string? adminIp, CancellationToken cancellationToken = default);
}
using Trackwise.Domain.Entities;

namespace Trackwise.Application.Admin;

public interface IBannedIpRepository
{
    Task<List<BannedIp>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<BannedIp?> GetByIpAsync(string ip, CancellationToken cancellationToken = default);
    void Add(BannedIp entry);
    void Remove(BannedIp entry);
}
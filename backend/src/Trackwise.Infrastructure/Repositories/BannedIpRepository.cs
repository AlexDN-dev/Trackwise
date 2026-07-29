using Microsoft.EntityFrameworkCore;
using Trackwise.Application.Admin;
using Trackwise.Domain.Entities;
using Trackwise.Infrastructure.Persistence;

namespace Trackwise.Infrastructure.Repositories;

public class BannedIpRepository : IBannedIpRepository
{
    private readonly AppDbContext _db;

    public BannedIpRepository(AppDbContext db) => _db = db;

    public Task<List<BannedIp>> GetAllAsync(CancellationToken cancellationToken = default) =>
        _db.BannedIps.OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);

    public Task<BannedIp?> GetByIpAsync(string ip, CancellationToken cancellationToken = default) =>
        _db.BannedIps.FirstOrDefaultAsync(x => x.IpAddress == ip, cancellationToken);

    public void Add(BannedIp entry) => _db.BannedIps.Add(entry);

    public void Remove(BannedIp entry) => _db.BannedIps.Remove(entry);
}
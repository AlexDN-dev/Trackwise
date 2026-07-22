using Microsoft.EntityFrameworkCore;
using Trackwise.Application.Auth;
using Trackwise.Domain.Entities;
using Trackwise.Infrastructure.Persistence;

namespace Trackwise.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _context;

    public RefreshTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<RefreshToken?> GetByHashedTokenAsync(string hashedToken, CancellationToken cancellationToken = default)
        => _context.RefreshTokens.Include(t => t.User).FirstOrDefaultAsync(t => t.Token == hashedToken, cancellationToken);

    public void Add(RefreshToken refreshToken)
        => _context.RefreshTokens.Add(refreshToken);

    public Task<List<RefreshToken>> GetActiveByUserAsync(Guid userId, CancellationToken cancellationToken = default)
        => _context.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null && t.ExpiresAt > DateTimeOffset.UtcNow)
            .ToListAsync(cancellationToken);
}
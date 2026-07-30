using Microsoft.EntityFrameworkCore;
using Trackwise.Application.Auth;
using Trackwise.Domain.Entities;
using Trackwise.Domain.Enums;
using Trackwise.Infrastructure.Persistence;

namespace Trackwise.Infrastructure.Repositories;

public class EmailTokenRepository : IEmailTokenRepository
{
    private readonly AppDbContext _context;

    public EmailTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    public void Add(EmailToken token)
        => _context.EmailTokens.Add(token);

    public Task<EmailToken?> GetByHashAsync(string tokenHash, EmailTokenType type, CancellationToken cancellationToken = default)
        => _context.EmailTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash && t.Type == type, cancellationToken);

    public Task<List<EmailToken>> GetActiveByUserAsync(Guid userId, EmailTokenType type, CancellationToken cancellationToken = default)
        => _context.EmailTokens
            .Where(t => t.UserId == userId && t.Type == type
                                           && t.UsedAt == null && t.ExpiresAt > DateTimeOffset.UtcNow)
            .ToListAsync(cancellationToken);
}
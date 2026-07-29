using Microsoft.EntityFrameworkCore;
using Trackwise.Application.Auth;
using Trackwise.Domain.Entities;
using Trackwise.Infrastructure.Persistence;

namespace Trackwise.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => _context.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<User?> GetByStripeCustomerIdAsync(string stripeCustomerId,
        CancellationToken cancellationToken = default)
        => _context.Users.FirstOrDefaultAsync(u => u.StripeCustomerId == stripeCustomerId, cancellationToken);

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
        => _context.Users.AnyAsync(u => u.Email == email, cancellationToken);

    public void Add(User user)
        => _context.Add(user);

    public async Task<(List<User> Items, int Total)> GetPagedAsync(int page, int size, string? search,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(u =>
                u.Email.ToLower().Contains(term)
                || u.Firstname.ToLower().Contains(term)
                || u.Lastname.ToLower().Contains(term));
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken);

        return (items, total);
    }
}
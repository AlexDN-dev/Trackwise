using Trackwise.Domain.Entities;

namespace Trackwise.Application.Auth;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByStripeCustomerIdAsync(string stripeCustomerId, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    void Add(User user);
    Task<(List<User> Items, int Total)> GetPagedAsync(
        int page, int size, string? search, CancellationToken cancellationToken = default);
}
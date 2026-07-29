using Trackwise.Application.Admin.Dtos;

namespace Trackwise.Application.Admin;

public interface IAdminUserService
{
    Task<AdminUsersPage> ListAsync(int page, int size, string? search, CancellationToken cancellationToken = default);
    Task<AdminUserDetail?> GetAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> BanAsync(Guid adminId, Guid userId, string? ipAddress, CancellationToken cancellationToken = default);
    Task<bool> UnbanAsync(Guid adminId, Guid userId, string? ipAddress, CancellationToken cancellationToken = default);
    Task<bool> GrantTrialAsync(Guid adminId, Guid userId, int days, string? ipAddress, CancellationToken cancellationToken = default);
    Task<bool> GrantTokensAsync(Guid adminId, Guid userId, int count, string? ipAddress, CancellationToken cancellationToken = default);
}
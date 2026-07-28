using Trackwise.Application.Users.Dtos;

namespace Trackwise.Application.Users;

public interface IUserService
{
    Task<UserResponse?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<UserResponse?> UpdateAsync(Guid userId, UpdateUserRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request,
        CancellationToken cancellationToken = default);
}
using Trackwise.Application.Auth;
using Trackwise.Application.Common.Interfaces;
using Trackwise.Application.Users.Dtos;
using Trackwise.Domain.Entities;

namespace Trackwise.Application.Users.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UserResponse?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        return user is null ? null : ToResponse(user);
    }

    public async Task<UserResponse?> UpdateAsync(Guid userId, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null) return null;

        user.Firstname = request.Firstname.Trim();
        user.Lastname = request.Lastname.Trim();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ToResponse(user);
    }
    
    private static UserResponse ToResponse(User u) => new()
    {
        Id = u.Id,
        Firstname = u.Firstname,
        Lastname = u.Lastname,
        Email = u.Email,
        PlanType = u.PlanType,
        CreatedAt = u.CreatedAt,
    };

}
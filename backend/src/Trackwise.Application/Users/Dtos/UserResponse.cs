using Trackwise.Domain.Enums;

namespace Trackwise.Application.Users.Dtos;

public class UserResponse
{
    public Guid Id { get; set; }
    public string Firstname { get; set; } = null!;
    public string Lastname { get; set; } = null!;
    public string Email { get; set; } = null!;
    public PlanType PlanType { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
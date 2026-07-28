using System.ComponentModel.DataAnnotations;
using Trackwise.Application.Common.Validation;

namespace Trackwise.Application.Users.Dtos;

public class ChangePasswordRequest
{
    [Required, MaxLength(100)]
    public string CurrentPassword { get; set; } = null!;

    [Required, StrongPassword, MaxLength(100)]
    public string NewPassword { get; set; } = null!;
}
using System.ComponentModel.DataAnnotations;
using Trackwise.Application.Common.Validation;

namespace Trackwise.Application.Auth.Dtos;

public class ResetPasswordRequest
{
    [Required, MaxLength(200)]
    public string Token { get; set; } = null!;
    [Required, StrongPassword, MaxLength(200)]
    public string NewPassword { get; set; } = null!;
}
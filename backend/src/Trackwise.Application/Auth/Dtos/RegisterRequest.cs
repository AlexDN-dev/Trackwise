using System.ComponentModel.DataAnnotations;
using Trackwise.Application.Common.Validation;

namespace Trackwise.Application.Auth.Dtos;

public class RegisterRequest
{
    [Required, MaxLength(100)]
    public string Firstname { get; set; } = null!;

    [Required, MaxLength(100)]
    public string Lastname { get; set; } = null!;

    [Required, ValidEmail, MaxLength(256)]
    public string Email { get; set; } = null!;

    [Required, StrongPassword, MaxLength(100)]
    public string Password { get; set; } = null!;
}
using System.ComponentModel.DataAnnotations;

namespace Trackwise.Application.Auth.Dtos;

public class RegisterRequest
{
    [Required, MaxLength(100)]
    public string Firstname { get; set; } = null!;

    [Required, MaxLength(100)]
    public string Lastname { get; set; } = null!;

    [Required, EmailAddress, MaxLength(256)]
    public string Email { get; set; } = null!;

    [Required, MinLength(8), MaxLength(100)]
    public string Password { get; set; } = null!;
}
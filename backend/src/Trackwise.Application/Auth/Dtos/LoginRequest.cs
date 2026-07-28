using System.ComponentModel.DataAnnotations;

namespace Trackwise.Application.Auth.Dtos;

public class LoginRequest
{
    [Required, EmailAddress, MaxLength(256)]
    public string Email { get; set; } = null!;
    [Required, MaxLength(100)]
    public string Password { get; set; } = null!;
}
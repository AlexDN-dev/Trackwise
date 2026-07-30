using System.ComponentModel.DataAnnotations;

namespace Trackwise.Application.Auth.Dtos;

public class ConfirmEmailRequest
{
    [Required, MaxLength(200)]
    public string Token { get; set; } = null!;
}
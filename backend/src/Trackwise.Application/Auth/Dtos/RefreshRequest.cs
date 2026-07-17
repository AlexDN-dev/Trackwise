using System.ComponentModel.DataAnnotations;

namespace Trackwise.Application.Auth.Dtos;

public class RefreshRequest
{
    [Required]
    public string RefreshToken { get; set; } = null!;
}
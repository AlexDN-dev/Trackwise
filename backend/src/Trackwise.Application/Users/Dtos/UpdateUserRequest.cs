using System.ComponentModel.DataAnnotations;

namespace Trackwise.Application.Users.Dtos;

public class UpdateUserRequest
{
    [Required, MaxLength(100)] public string Firstname { get; set; } = null!;
    [Required, MaxLength(100)] public string Lastname { get; set; } = null!;
}
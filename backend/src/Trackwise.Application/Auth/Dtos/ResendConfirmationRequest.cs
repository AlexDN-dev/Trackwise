using System.ComponentModel.DataAnnotations;
using Trackwise.Application.Common.Validation;

namespace Trackwise.Application.Auth.Dtos;

public class ResendConfirmationRequest
{
    [Required, ValidEmail, MaxLength(256)]
    public string Email { get; set; } = null!;
}
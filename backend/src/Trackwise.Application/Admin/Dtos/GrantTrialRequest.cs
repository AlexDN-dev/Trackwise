using System.ComponentModel.DataAnnotations;

namespace Trackwise.Application.Admin.Dtos;

public class GrantTrialRequest
{
    [Required, Range(1, 365)]
    public int Days { get; set; }
}
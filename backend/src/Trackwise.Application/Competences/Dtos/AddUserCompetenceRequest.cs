using System.ComponentModel.DataAnnotations;

namespace Trackwise.Application.Competences.Dtos;

public class AddUserCompetenceRequest
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = null!;
}
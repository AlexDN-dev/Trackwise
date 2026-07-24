using System.ComponentModel.DataAnnotations;

namespace Trackwise.Application.AI.Dtos;

public class AnalyzeRequest
{
    [Required, MinLength(50), MaxLength(20000)]
    public string Text { get; set; } = null!;
}
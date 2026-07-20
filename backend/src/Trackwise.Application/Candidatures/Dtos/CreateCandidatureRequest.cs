using System.ComponentModel.DataAnnotations;
using Trackwise.Domain.Enums;

namespace Trackwise.Application.Candidatures.Dtos;

public class CreateCandidatureRequest : IValidatableObject
{
    [Required, MaxLength(100)]
    public string Title { get; set; } = null!;
    [MaxLength(100)]
    public string? Location { get; set; }
    [MaxLength(256)]
    public string? UrlOffer { get; set; }
    [MaxLength(2000)]
    public string? Description { get; set; }
    public ContractType ContractType { get; set; }
    public DateOnly? PostulationDate { get; set; }
    public Guid? EntrepriseId { get; set; }
    
    [MaxLength(150)]
    public string? EntrepriseName { get; set; }
    
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (EntrepriseId is null && string.IsNullOrWhiteSpace(EntrepriseName))
        {
            yield return new ValidationResult(
                "Choisis une entreprise existante ou saisis-en une nouvelle.",
                new[] { nameof(EntrepriseId), nameof(EntrepriseName) });
        }
    }

}
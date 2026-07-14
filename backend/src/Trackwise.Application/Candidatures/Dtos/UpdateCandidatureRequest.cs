using System.ComponentModel.DataAnnotations;
using Trackwise.Domain.Enums;

namespace Trackwise.Application.Candidatures.Dtos;

public class UpdateCandidatureRequest
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
    public CandidatureStatus Status { get; set; }
    public DateOnly? PostulationDate { get; set; }
    
    [Required]
    public Guid EntrepriseId { get; set; }
}
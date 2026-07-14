using Trackwise.Domain.Enums;

namespace Trackwise.Application.Candidatures.Dtos;

public class CandidatureResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public CandidatureStatus Status { get; set; }
    public string? Location { get; set; }
    public string? UrlOffer { get; set; }
    public string? Description { get; set; }
    public ContractType ContractType { get; set; }
    public DateOnly? PostulationDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    
    public Guid EntrepriseId { get; set; }
    public string? EntrepriseName { get; set; }
}
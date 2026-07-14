using Trackwise.Domain.Common;
using Trackwise.Domain.Enums;

namespace Trackwise.Domain.Entities;

public class Candidature : BaseEntity
{
    public required string Title { get; set; }
    public required CandidatureStatus Status { get; set; }
    public DateOnly? PostulationDate { get; set; }
    public string? Location { get; set; }
    public string? UrlOffer { get; set; }
    public string? Description { get; set; }
    public ContractType ContractType { get; set; }
    
    public Guid EntrepriseId { get; set; }
    public Entreprise Entreprise { get; set; } = null!;
    
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public ICollection<Competence> Competences { get; set; } = new List<Competence>();
}
using Trackwise.Domain.Common;

namespace Trackwise.Domain.Entities;

public class Entreprise : BaseEntity
{
    public required string Name { get; set; }
    public string? City { get; set; }
    public string? Sector { get; set; }
    public string? Website { get; set; }

    public ICollection<Candidature> Candidatures { get; set; } = new List<Candidature>();
}
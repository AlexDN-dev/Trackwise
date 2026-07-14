using Trackwise.Domain.Common;

namespace Trackwise.Domain.Entities;

public class Competence : BaseEntity
{
    public required string Name { get; set; }
    public string? NormalizedName { get; set; }

    public ICollection<Candidature> Candidatures { get; set; } = new List<Candidature>();
}
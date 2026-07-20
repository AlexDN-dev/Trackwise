namespace Trackwise.Application.Entreprises.Dtos;

public class EntrepriseResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Sector { get; set; }
    public string? City { get; set; }
    public string? Website { get; set; }
}
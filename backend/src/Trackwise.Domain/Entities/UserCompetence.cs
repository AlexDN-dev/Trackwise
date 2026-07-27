namespace Trackwise.Domain.Entities;

public class UserCompetence
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid CompetenceId { get; set; }
    public Competence Competence { get; set; } = null!;
    
    public DateTimeOffset CreatedAt { get; set; }
}
namespace Trackwise.Application.Admin.Dtos;

public class AdminStatsResponse
{
    public int TotalUsers { get; set; }
    public int FreeUsers { get; set; }
    public int PremiumUsers { get; set; }
    public int BannedUsers { get; set; }
    public int Admins { get; set; }
    public int TotalCandidatures { get; set; }
    public int TotalAiAnalysesThisMonth { get; set; }
    public int SignupsLast30Days { get; set; }
    public int SignupsLast7Days { get; set; }
}
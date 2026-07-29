namespace Trackwise.Application.Admin.Dtos;

public class AdminUsersPage
{
    public List<AdminUserListItem> Items { get; set; } = new();
    public int Total { get; set; }
    public int Page { get; set; }
    public int Size { get; set; }
}
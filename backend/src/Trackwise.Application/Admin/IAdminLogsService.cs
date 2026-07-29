using Trackwise.Application.Admin.Dtos;

namespace Trackwise.Application.Admin;

public interface IAdminLogsService
{
    Task<LogsResponse> ReadRecentAsync(int lines, string? levelFilter, string? searchFilter, CancellationToken cancellationToken = default);
}
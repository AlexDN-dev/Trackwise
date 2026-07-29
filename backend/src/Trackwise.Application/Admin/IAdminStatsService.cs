using Trackwise.Application.Admin.Dtos;

namespace Trackwise.Application.Admin;

public interface IAdminStatsService
{
    Task<AdminStatsResponse> GetAsync(CancellationToken cancellationToken = default);
}
using TalentManagementAPI.Application.Features.Dashboard.Queries.GetDashboardMetrics;

namespace TalentManagementAPI.Application.Interfaces
{
    public interface IDashboardMetricsReader
    {
        Task<DashboardMetricsDto> GetDashboardMetricsAsync(CancellationToken ct);
    }

}

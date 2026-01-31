using TalentManagementData.Application.Features.Dashboard.Queries.GetDashboardMetrics;

namespace TalentManagementData.Application.Interfaces
{
    public interface IDashboardMetricsReader
    {
        Task<DashboardMetricsDto> GetDashboardMetricsAsync(CancellationToken ct);
    }

}

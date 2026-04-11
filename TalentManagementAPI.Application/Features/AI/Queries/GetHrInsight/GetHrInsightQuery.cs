#nullable enable
using System.Diagnostics;
using System.Text;
using TalentManagementAPI.Application.Features.Dashboard.Queries.GetDashboardMetrics;
using TalentManagementAPI.Application.Interfaces;

namespace TalentManagementAPI.Application.Features.AI.Queries.GetHrInsight
{
    public sealed class GetHrInsightQuery : IRequest<Result<HrInsightDto>>
    {
        public string Question { get; init; } = string.Empty;

        public sealed class GetHrInsightQueryHandler
            : IRequestHandler<GetHrInsightQuery, Result<HrInsightDto>>
        {
            private readonly IDashboardMetricsReader _metricsReader;
            private readonly IAiChatService _aiChatService;

            public GetHrInsightQueryHandler(
                IDashboardMetricsReader metricsReader,
                IAiChatService aiChatService)
            {
                _metricsReader = metricsReader;
                _aiChatService = aiChatService;
            }

            public async Task<Result<HrInsightDto>> Handle(
                GetHrInsightQuery request,
                CancellationToken cancellationToken)
            {
                var start = Stopwatch.GetTimestamp();

                var metrics = await _metricsReader
                    .GetDashboardMetricsAsync(cancellationToken)
                    .ConfigureAwait(false);

                var systemPrompt = BuildSystemPrompt(metrics);

                var answer = await _aiChatService
                    .ChatAsync(request.Question, systemPrompt, cancellationToken)
                    .ConfigureAwait(false);

                var elapsed = (long)Stopwatch.GetElapsedTime(start).TotalMilliseconds;

                return Result<HrInsightDto>.Success(new HrInsightDto
                {
                    Question = request.Question,
                    Answer = answer,
                    ExecutionTimeMs = elapsed
                });
            }

            private static string BuildSystemPrompt(DashboardMetricsDto metrics)
            {
                var sb = new StringBuilder();
                sb.AppendLine("You are an HR data analyst assistant. Answer questions using only the workforce data provided below. Be concise and factual.");
                sb.AppendLine();
                sb.AppendLine("=== CURRENT WORKFORCE DATA ===");
                sb.AppendLine($"Total Employees: {metrics.TotalEmployees}");
                sb.AppendLine($"Total Departments: {metrics.TotalDepartments}");
                sb.AppendLine($"Total Positions: {metrics.TotalPositions}");
                sb.AppendLine($"Total Salary Ranges: {metrics.TotalSalaryRanges}");
                sb.AppendLine($"New Hires This Month: {metrics.NewHiresThisMonth}");
                sb.AppendLine($"Average Salary: {metrics.AverageSalary:C}");
                sb.AppendLine($"Gender Distribution: {metrics.GenderDistribution.Male} male, {metrics.GenderDistribution.Female} female");
                sb.AppendLine();

                if (metrics.EmployeesByDepartment.Count > 0)
                {
                    sb.AppendLine("Employees by Department:");
                    foreach (var d in metrics.EmployeesByDepartment)
                        sb.AppendLine($"  - {d.DepartmentName}: {d.EmployeeCount}");
                    sb.AppendLine();
                }

                if (metrics.EmployeesByPosition.Count > 0)
                {
                    sb.AppendLine("Employees by Position:");
                    foreach (var p in metrics.EmployeesByPosition)
                        sb.AppendLine($"  - {p.PositionTitle}: {p.EmployeeCount}");
                    sb.AppendLine();
                }

                if (metrics.RecentEmployees.Count > 0)
                {
                    sb.AppendLine("Recent Hires (last 5):");
                    foreach (var e in metrics.RecentEmployees)
                        sb.AppendLine($"  - {e.FullName} ({e.PositionTitle}, {e.DepartmentName}) — hired {e.CreatedAt:yyyy-MM-dd}");
                }

                return sb.ToString();
            }
        }
    }
}

#nullable enable
namespace TalentManagementAPI.Application.Features.AI.Queries.GetHrInsight
{
    public sealed class HrInsightDto
    {
        public string Question { get; init; } = string.Empty;
        public string Answer { get; init; } = string.Empty;
        public long ExecutionTimeMs { get; init; }
    }
}

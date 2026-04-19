namespace TalentManagementAPI.Application.Features.Positions.Queries.SemanticSearch
{
    public sealed class SemanticPositionResultDto
    {
        public Guid   Id                  { get; init; }
        public string PositionNumber      { get; init; } = string.Empty;
        public string PositionTitle       { get; init; } = string.Empty;
        public string PositionDescription { get; init; } = string.Empty;
        public string DepartmentName      { get; init; } = string.Empty;
        public string SalaryRangeName     { get; init; } = string.Empty;
        public double Score               { get; init; }
        public long   ExecutionTimeMs     { get; init; }
    }
}

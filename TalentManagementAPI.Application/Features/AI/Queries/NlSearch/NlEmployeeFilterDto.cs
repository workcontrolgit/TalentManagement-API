#nullable enable
namespace TalentManagementAPI.Application.Features.AI.Queries.NlSearch
{
    /// <summary>
    /// The structured employee filter parsed from a natural language query.
    /// Each string field maps directly to a parameter accepted by GET /api/v1/employees.
    /// </summary>
    public sealed class NlEmployeeFilterDto
    {
        public string OriginalQuery    { get; init; } = string.Empty;
        public string FirstName        { get; init; } = string.Empty;
        public string LastName         { get; init; } = string.Empty;
        public string Email            { get; init; } = string.Empty;
        public string EmployeeNumber   { get; init; } = string.Empty;
        public string PositionTitle    { get; init; } = string.Empty;
        public string ParsedExpression { get; init; } = string.Empty;
        public long   ExecutionTimeMs  { get; init; }
    }
}

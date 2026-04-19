using TalentManagementAPI.Application.Common.Results;
using TalentManagementAPI.Application.Messaging;

namespace TalentManagementAPI.Application.Features.Positions.Queries.SemanticSearch
{
    public sealed class SemanticPositionSearchQuery : IRequest<Result<List<SemanticPositionResultDto>>>
    {
        public string QueryText { get; init; } = string.Empty;
        public int TopK { get; init; } = 10;
    }
}

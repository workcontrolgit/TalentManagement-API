#nullable enable
using System.Diagnostics;
using Microsoft.Data.SqlTypes;
using Microsoft.EntityFrameworkCore;
using TalentManagementAPI.Application.Common.Results;
using TalentManagementAPI.Application.Features.Positions.Queries.SemanticSearch;
using TalentManagementAPI.Application.Interfaces;
using TalentManagementAPI.Application.Messaging;
using TalentManagementAPI.Infrastructure.Persistence.Contexts;

namespace TalentManagementAPI.Infrastructure.Persistence.QueryHandlers
{
    public sealed class SemanticPositionSearchQueryHandler
        : IRequestHandler<SemanticPositionSearchQuery, Result<List<SemanticPositionResultDto>>>
    {
        private readonly IEmbeddingService _embeddingService;
        private readonly ApplicationDbContext _dbContext;

        public SemanticPositionSearchQueryHandler(
            IEmbeddingService embeddingService,
            ApplicationDbContext dbContext)
        {
            _embeddingService = embeddingService;
            _dbContext = dbContext;
        }

        public async Task<Result<List<SemanticPositionResultDto>>> Handle(
            SemanticPositionSearchQuery request,
            CancellationToken cancellationToken)
        {
            var sw = Stopwatch.StartNew();

            float[] queryVector;
            try
            {
                queryVector = await _embeddingService
                    .EmbedAsync(request.QueryText, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Result<List<SemanticPositionResultDto>>.Failure(
                    $"Embedding service unavailable: {ex.Message}");
            }

            var sqlVector = new SqlVector<float>(queryVector);

            var rows = await _dbContext.Positions
                .Where(p => p.SearchEmbedding != null)
                .Include(p => p.Department)
                .Include(p => p.SalaryRange)
                .Select(p => new
                {
                    Position = p,
                    Distance = EF.Functions.VectorDistance(
                        "cosine",
                        new SqlVector<float>(p.SearchEmbedding!),
                        sqlVector)
                })
                .OrderBy(x => x.Distance)
                .Take(request.TopK)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            sw.Stop();

            var dtos = rows.Select(r => new SemanticPositionResultDto
            {
                Id                  = r.Position.Id,
                PositionNumber      = r.Position.PositionNumber,
                PositionTitle       = r.Position.PositionTitle?.Value ?? string.Empty,
                PositionDescription = r.Position.PositionDescription,
                DepartmentName      = r.Position.Department?.Name?.Value ?? string.Empty,
                SalaryRangeName     = r.Position.SalaryRange?.Name ?? string.Empty,
                Score               = Math.Round(1.0 - r.Distance, 4),
                ExecutionTimeMs     = sw.ElapsedMilliseconds,
            }).ToList();

            return Result<List<SemanticPositionResultDto>>.Success(dtos);
        }
    }
}

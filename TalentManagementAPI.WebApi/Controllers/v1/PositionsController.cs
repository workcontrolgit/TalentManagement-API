using System.ComponentModel.DataAnnotations;
using TalentManagementAPI.Application.Features.Positions.Queries.SemanticSearch;
using TalentManagementAPI.Application.Interfaces;
using TalentManagementAPI.Infrastructure.Persistence.Contexts;

namespace TalentManagementAPI.WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    public class PositionsController : BaseApiController
    {

        /// <summary>
        /// Gets a list of positions based on the provided filter.
        /// </summary>
        /// <param name="filter">The filter used to query the positions.</param>
        /// <returns>A list of positions.</returns>
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Get([FromQuery] GetPositionsQuery filter)
        {
            return Ok(await Mediator.Send(filter));
        }

        /// <summary>
        /// Gets a position by its Id.
        /// </summary>
        /// <param name="id">The Id of the position.</param>
        /// <returns>The position with the specified Id.</returns>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> Get(Guid id)
        {
            return Ok(await Mediator.Send(new GetPositionByIdQuery { Id = id }));
        }

        /// <summary>
        /// Creates a new position.
        /// </summary>
        /// <param name="command">The command containing the data for the new position.</param>
        /// <returns>A 201 Created response containing the newly created position.</returns>
        [HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Post(CreatePositionCommand command)
        {
            var resp = await Mediator.Send(command);
            return CreatedAtAction(nameof(Post), resp);
        }

        /// <summary>
        /// Sends an InsertMockPositionCommand to the mediator.
        /// </summary>
        /// <param name="command">The command to be sent.</param>
        /// <returns>The result of the command.</returns>
        [HttpPost]
        [Route("AddMock")]
        [Authorize]
        public async Task<IActionResult> AddMock(InsertMockPositionCommand command)
        {
            return Ok(await Mediator.Send(command));
        }

        /// <summary>
        /// Updates a position with the given id using the provided command.
        /// </summary>
        /// <param name="id">The id of the position to update.</param>
        /// <param name="command">The command containing the updated information.</param>
        /// <returns>The updated position.</returns>
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Put(Guid id, UpdatePositionCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest();
            }
            return Ok(await Mediator.Send(command));
        }

        /// <summary>
        /// Deletes a position by its Id.
        /// </summary>
        /// <param name="id">The Id of the position to delete.</param>
        /// <returns>The result of the deletion.</returns>
        [HttpDelete("{id}")]
        [Authorize(Policy = AuthorizationConsts.AdminPolicy)]
        public async Task<IActionResult> Delete(Guid id)
        {
            return Ok(await Mediator.Send(new DeletePositionByIdCommand { Id = id }));
        }

        /// <summary>
        /// Finds positions semantically similar to the query text using vector distance.
        /// Requires VectorSearchEnabled feature flag and populated SearchEmbedding values.
        /// </summary>
        [HttpPost("semantic-search")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<SemanticPositionResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> SemanticSearch(
            [FromBody] SemanticPositionSearchRequest request,
            [FromServices] IFeatureManagerSnapshot featureManager,
            CancellationToken cancellationToken)
        {
            if (!await featureManager.IsEnabledAsync("VectorSearchEnabled"))
                return Problem(
                    detail: "Vector search is disabled. Enable FeatureManagement:VectorSearchEnabled to use this endpoint.",
                    title: "Vector search is disabled",
                    statusCode: StatusCodes.Status503ServiceUnavailable);

            var result = await Mediator.Send(
                new SemanticPositionSearchQuery { QueryText = request.QueryText, TopK = request.TopK },
                cancellationToken);

            return result.IsSuccess
                ? Ok(result.Value)
                : BadRequest(new { detail = result.Errors.FirstOrDefault() });
        }

        /// <summary>
        /// Generates and stores SearchEmbedding for all positions that do not have one yet.
        /// Run once after seeding data to enable semantic search.
        /// </summary>
        [HttpPost("generate-embeddings")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> GenerateEmbeddings(
            [FromServices] IFeatureManagerSnapshot featureManager,
            [FromServices] IEmbeddingService embeddingService,
            [FromServices] ApplicationDbContext dbContext,
            CancellationToken cancellationToken)
        {
            if (!await featureManager.IsEnabledAsync("VectorSearchEnabled"))
                return Problem(
                    detail: "Vector search is disabled.",
                    title: "Vector search is disabled",
                    statusCode: StatusCodes.Status503ServiceUnavailable);

            var positions = dbContext.Positions
                .Where(p => p.SearchEmbedding == null)
                .ToList();

            foreach (var position in positions)
            {
                var text = $"{position.PositionTitle?.Value} {position.PositionDescription}".Trim();
                position.SearchEmbedding = await embeddingService.EmbedAsync(text, cancellationToken);
            }

            dbContext.Positions.UpdateRange(positions);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Ok(new { generated = positions.Count });
        }
    }

    public record SemanticPositionSearchRequest(
        [Required][MinLength(3)] string QueryText,
        int TopK = 10);
}


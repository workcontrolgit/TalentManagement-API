using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TalentManagementAPI.Application.Features.AI.Queries.GetHrInsight;
using TalentManagementAPI.Application.Interfaces;

namespace TalentManagementAPI.WebApi.Controllers.v1
{
    [ApiVersion("1.0")]
    [AllowAnonymous]
    [Route("api/v{version:apiVersion}/ai")]
    public sealed class AiController : BaseApiController
    {
        private readonly IAiChatService _aiChatService;
        private readonly IFeatureManagerSnapshot _featureManager;

        public AiController(IAiChatService aiChatService, IFeatureManagerSnapshot featureManager)
        {
            _aiChatService = aiChatService;
            _featureManager = featureManager;
        }

        /// <summary>
        /// Send a message to the AI assistant and receive a reply.
        /// </summary>
        /// <param name="request">The chat message and optional system prompt.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The AI-generated reply.</returns>
        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] AiChatRequest request, CancellationToken cancellationToken)
        {
            if (!await _featureManager.IsEnabledAsync("AiEnabled"))
            {
                return Problem(
                    detail: "AI chat is disabled. Enable FeatureManagement:AiEnabled to use this endpoint.",
                    title: "AI chat is disabled",
                    statusCode: StatusCodes.Status503ServiceUnavailable);
            }

            var reply = await _aiChatService.ChatAsync(request.Message, request.SystemPrompt, cancellationToken);
            return Ok(new AiChatResponse(reply));
        }

        /// <summary>
        /// Ask the HR AI assistant a question about your current workforce data.
        /// The assistant fetches live dashboard metrics and injects them into the prompt context.
        /// </summary>
        /// <param name="request">The HR question to answer.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>An AI-generated answer grounded in real workforce data.</returns>
        [HttpPost("hr-insight")]
        public async Task<IActionResult> HrInsight([FromBody] HrInsightRequest request, CancellationToken cancellationToken)
        {
            if (!await _featureManager.IsEnabledAsync("AiEnabled"))
            {
                return Problem(
                    detail: "AI features are disabled. Enable FeatureManagement:AiEnabled to use this endpoint.",
                    title: "AI is disabled",
                    statusCode: StatusCodes.Status503ServiceUnavailable);
            }

            var result = await Mediator.Send(new GetHrInsightQuery { Question = request.Question }, cancellationToken);
            return Ok(result);
        }
    }

    public record AiChatRequest(string Message, string? SystemPrompt = null);
    public record AiChatResponse(string Reply);
    public record HrInsightRequest(string Question);
}

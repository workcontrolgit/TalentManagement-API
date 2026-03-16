using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using TalentManagementAPI.Application.Interfaces;

namespace TalentManagementAPI.WebApi.Controllers.v1
{
    [FeatureGate("AiEnabled")]
    [ApiVersion("1.0")]
    [AllowAnonymous]
    [Route("api/v{version:apiVersion}/ai")]
    public sealed class AiController : BaseApiController
    {
        private readonly IAiChatService _aiChatService;

        public AiController(IAiChatService aiChatService)
        {
            _aiChatService = aiChatService;
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
            var reply = await _aiChatService.ChatAsync(request.Message, request.SystemPrompt, cancellationToken);
            return Ok(new AiChatResponse(reply));
        }
    }

    public record AiChatRequest(string Message, string? SystemPrompt = null);
    public record AiChatResponse(string Reply);
}

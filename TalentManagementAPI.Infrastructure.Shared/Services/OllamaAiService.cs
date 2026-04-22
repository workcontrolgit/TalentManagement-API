#nullable enable
using Microsoft.Extensions.AI;
using TalentManagementAPI.Application.Interfaces;

namespace TalentManagementAPI.Infrastructure.Shared.Services
{
    public class OllamaAiService : IAiChatService
    {
        private readonly IChatClient _chatClient;

        public OllamaAiService(IChatClient chatClient)
        {
            _chatClient = chatClient;
        }

        public async Task<string> ChatAsync(string message, string? systemPrompt = null,
            CancellationToken cancellationToken = default)
        {
            var messages = new List<ChatMessage>();

            if (!string.IsNullOrWhiteSpace(systemPrompt))
                messages.Add(new ChatMessage(Microsoft.Extensions.AI.ChatRole.System, systemPrompt));

            messages.Add(new ChatMessage(Microsoft.Extensions.AI.ChatRole.User, message));

            var response = await _chatClient.GetResponseAsync(messages, cancellationToken: cancellationToken);
            return response.Text ?? string.Empty;
        }
    }
}

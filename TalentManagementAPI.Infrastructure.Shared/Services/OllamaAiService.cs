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

        public async Task<string> ChatAsync(string message, string? systemPrompt = null, CancellationToken cancellationToken = default)
        {
            var messages = new List<ChatMessage>();

            if (!string.IsNullOrWhiteSpace(systemPrompt))
                messages.Add(new ChatMessage(ChatRole.System, systemPrompt));

            messages.Add(new ChatMessage(ChatRole.User, message));

            var response = await _chatClient.CompleteAsync(messages, cancellationToken: cancellationToken);
            return response.Message.Text ?? string.Empty;
        }
    }
}

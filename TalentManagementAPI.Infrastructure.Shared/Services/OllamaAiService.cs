using TalentManagementAPI.Application.Interfaces;

namespace TalentManagementAPI.Infrastructure.Shared.Services
{
    public class OllamaAiService : IAiChatService
    {
        private readonly IOllamaApiClient _ollamaApiClient;

        public OllamaAiService(IOllamaApiClient ollamaApiClient)
        {
            _ollamaApiClient = ollamaApiClient;
        }

        public async Task<string> ChatAsync(string message, string? systemPrompt = null, CancellationToken cancellationToken = default)
        {
            var messages = new List<Message>();

            if (!string.IsNullOrWhiteSpace(systemPrompt))
            {
                messages.Add(new Message(new ChatRole("system"), systemPrompt));
            }

            messages.Add(new Message(new ChatRole("user"), message));

            var request = new ChatRequest
            {
                Model = _ollamaApiClient.SelectedModel,
                Messages = messages,
                Stream = true
            };

            var responseBuilder = new MessageBuilder();

            await foreach (var response in _ollamaApiClient.ChatAsync(request, cancellationToken).WithCancellation(cancellationToken))
            {
                if (response?.Message is not null)
                {
                    responseBuilder.Append(response);
                }
            }

            return responseBuilder.HasValue ? responseBuilder.ToMessage().Content ?? string.Empty : string.Empty;
        }
    }
}

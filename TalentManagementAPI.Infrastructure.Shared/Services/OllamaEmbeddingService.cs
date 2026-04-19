using OllamaSharp;
using OllamaSharp.Models;
using TalentManagementAPI.Application.Interfaces;

namespace TalentManagementAPI.Infrastructure.Shared.Services
{
    /// <summary>
    /// Generates text embeddings using the Ollama /api/embed endpoint.
    /// Uses a dedicated embedding model (e.g. nomic-embed-text) separate from the chat model.
    /// </summary>
    public sealed class OllamaEmbeddingService : IEmbeddingService
    {
        private readonly IOllamaApiClient _client;
        private readonly string _embeddingModel;

        public OllamaEmbeddingService(IOllamaApiClient client, string embeddingModel)
        {
            _client = client;
            _embeddingModel = embeddingModel;
        }

        public async Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default)
        {
            var request = new EmbedRequest
            {
                Model = _embeddingModel,
                Input = new List<string> { text }
            };

            var response = await _client.EmbedAsync(request, cancellationToken).ConfigureAwait(false);

            if (response?.Embeddings is null || response.Embeddings.Count == 0)
                throw new InvalidOperationException(
                    $"Ollama returned no embeddings for model '{_embeddingModel}'.");

            return response.Embeddings[0].Select(d => (float)d).ToArray();
        }
    }
}

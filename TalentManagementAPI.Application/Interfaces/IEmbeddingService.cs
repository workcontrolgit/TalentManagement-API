namespace TalentManagementAPI.Application.Interfaces
{
    /// <summary>
    /// Generates a float[] embedding vector for a given text input.
    /// Implementations call a local or remote embedding model.
    /// </summary>
    public interface IEmbeddingService
    {
        Task<float[]> EmbedAsync(string text, CancellationToken cancellationToken = default);
    }
}

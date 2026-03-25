namespace TalentManagementAPI.Application.Interfaces
{
    public interface IAiChatService
    {
        Task<string> ChatAsync(string message, string? systemPrompt = null, CancellationToken cancellationToken = default);
    }
}

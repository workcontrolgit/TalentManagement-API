namespace TalentManagementAPI.Application.Interfaces
{
    /// <summary>
    /// Scoped service that carries AI response metadata (e.g., cache hit status)
    /// from the service layer to the controller within a single request.
    /// </summary>
    public interface IAiResponseMetadata
    {
        bool WasCacheHit { get; set; }
    }
}

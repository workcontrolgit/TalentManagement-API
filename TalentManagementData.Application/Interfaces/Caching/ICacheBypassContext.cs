#nullable enable
namespace TalentManagementData.Application.Interfaces.Caching
{
    public interface ICacheBypassContext
    {
        bool ShouldBypass { get; }

        string? Reason { get; }

        void Enable(string reason);

        void Reset();
    }

}

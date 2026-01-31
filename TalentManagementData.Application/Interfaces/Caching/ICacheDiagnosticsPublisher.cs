#nullable enable
namespace TalentManagementData.Application.Interfaces.Caching
{
    public interface ICacheDiagnosticsPublisher
    {
        void ReportHit(string cacheKey, TimeSpan? cacheDuration);

        void ReportMiss(string cacheKey, TimeSpan? cacheDuration);
    }

}

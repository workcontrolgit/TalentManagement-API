#nullable enable
namespace TalentManagementData.WebApi.Models
{
    public sealed record CacheStatsResponse(
        long Hits,
        long Misses,
        double HitRate,
        double AverageHitLatencyMs,
        double AverageMissLatencyMs,
        DateTimeOffset StartedAtUtc);

}

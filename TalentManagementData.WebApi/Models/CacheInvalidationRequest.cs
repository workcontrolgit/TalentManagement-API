#nullable enable
namespace TalentManagementData.WebApi.Models
{
    public sealed class CacheInvalidationRequest
    {
        public string? Key { get; set; }

        public string? Prefix { get; set; }

        public bool InvalidateAll { get; set; }
    }

}

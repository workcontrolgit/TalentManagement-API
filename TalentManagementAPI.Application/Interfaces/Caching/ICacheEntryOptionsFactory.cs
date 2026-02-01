namespace TalentManagementAPI.Application.Interfaces.Caching
{
    public interface ICacheEntryOptionsFactory
    {
        CacheEntryOptions Create(string endpointKey);
    }

}

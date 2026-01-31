namespace TalentManagementData.Application.Interfaces.Caching
{
    public interface ICacheKeyHasher
    {
        string Hash(string cacheKey);
    }

}

using Microsoft.AspNetCore.Builder;

namespace TalentManagementAPI.WebApi.Middlewares
{
    public static class CacheBypassMiddlewareExtensions
    {
        public static IApplicationBuilder UseCacheBypassMiddleware(this IApplicationBuilder builder)
            => builder.UseMiddleware<CacheBypassMiddleware>();
    }

}

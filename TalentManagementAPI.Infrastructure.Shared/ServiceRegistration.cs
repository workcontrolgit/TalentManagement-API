using Microsoft.Extensions.AI;
using TalentManagementAPI.Application.Interfaces;
using TalentManagementAPI.Application.Interfaces.Caching;
using TalentManagementAPI.Infrastructure.Shared.Services;

namespace TalentManagementAPI.Infrastructure.Shared
{
    public static class ServiceRegistration
    {
        public static void AddSharedInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<MailSettings>(config.GetSection("MailSettings"));
            services.AddTransient<IDateTimeService, DateTimeService>();
            services.AddTransient<IEmailService, EmailService>();
            services.AddTransient<IMockService, MockService>();
            // OllamaApiClient implements both IChatClient (Microsoft.Extensions.AI) and IOllamaApiClient.
            // Register as a singleton so both interfaces resolve to the same instance.
            services.AddSingleton<OllamaApiClient>(_ =>
            {
                var baseUrl = config["Ollama:BaseUrl"] ?? "http://localhost:11434";
                var model = config["Ollama:Model"] ?? "llama3.2";
                return new OllamaApiClient(new Uri(baseUrl), model);
            });
            services.AddSingleton<IChatClient>(sp => sp.GetRequiredService<OllamaApiClient>());
            services.AddSingleton<IOllamaApiClient>(sp => sp.GetRequiredService<OllamaApiClient>());

            services.AddScoped<IAiResponseMetadata, AiResponseMetadata>();

            var ttlMinutes = config.GetValue<int>("Ollama:CacheTtlMinutes", 60);
            var ttl = TimeSpan.FromMinutes(ttlMinutes);

            services.AddTransient<OllamaAiService>();
            services.AddTransient<IAiChatService>(sp => new CachingAiChatService(
                sp.GetRequiredService<OllamaAiService>(),
                sp.GetRequiredService<ICacheProvider>(),
                sp.GetRequiredService<IAiResponseMetadata>(),
                ttl));

            var embeddingModel = config["Ollama:EmbeddingModel"] ?? "nomic-embed-text";
            services.AddTransient<IEmbeddingService>(sp => new OllamaEmbeddingService(
                sp.GetRequiredService<IOllamaApiClient>(),
                embeddingModel));
        }
    }

    internal sealed class AiResponseMetadata : IAiResponseMetadata
    {
        public bool WasCacheHit { get; set; }
    }
}

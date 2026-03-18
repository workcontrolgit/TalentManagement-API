using TalentManagementAPI.Application.Interfaces;
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
            services.AddSingleton<IOllamaApiClient>(_ =>
            {
                var baseUrl = config["Ollama:BaseUrl"] ?? "http://localhost:11434";
                var model = config["Ollama:Model"] ?? "llama3.2";
                return new OllamaApiClient(new Uri(baseUrl), model);
            });
            services.AddTransient<IAiChatService, OllamaAiService>();
        }
    }
}

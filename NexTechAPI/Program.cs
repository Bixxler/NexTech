using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NextTech.Server.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;


var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services => {
        services.AddMemoryCache();
        services.AddHttpClient<IStoryService, StoryService>((serviceProvider, client) =>
        {
            var config = serviceProvider.GetRequiredService<IConfiguration>();
            var baseUrl = config.GetConnectionString("HackerNewsApi");

            client.BaseAddress = new Uri(baseUrl);
        });
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
    })
    .Build();

host.Run();

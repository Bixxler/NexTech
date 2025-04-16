using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NexTech.API.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Google.Protobuf.WellKnownTypes;


var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services => {
        services.AddMemoryCache();
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", builder =>
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader());
        });

        services.AddHttpClient<IStoryService, StoryService>((serviceProvider, client) =>
        {
            var config = serviceProvider.GetRequiredService<IConfiguration>();

            client.BaseAddress = new Uri("https://hacker-news.firebaseio.com/");
        });
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
    })
    .Build();

host.Run();

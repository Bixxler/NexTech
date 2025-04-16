using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NexTech.API.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
.SetBasePath(AppContext.BaseDirectory)
.AddJsonFile("app.settings.json", optional: true, reloadOnChange: true)
.AddEnvironmentVariables()
.Build();
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
        services.AddSingleton<IConfiguration>(configuration);
        services.AddHttpClient<IStoryService, StoryService>((serviceProvider, client) =>
        {
            client.BaseAddress = new Uri("https://hacker-news.firebaseio.com/");
        });
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
    })
    .Build();

host.Run();
public partial class Program { }

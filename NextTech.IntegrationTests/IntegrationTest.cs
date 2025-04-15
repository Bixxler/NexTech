using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using NextTech.Server.Models;
using NextTech.Server.Services;
using RichardSzalay.MockHttp;
namespace NextTech.IntegrationTests
{
   public class IntegrationTests: IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public IntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the existing service registration
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IStoryService));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    var mockHttp = new MockHttpMessageHandler();

                    // Mock the newstories response
                    mockHttp.When("https://hacker-news.firebaseio.com/v0/newstories.json")
                            .Respond("application/json", JsonSerializer.Serialize(new List<int> { 1, 2 }));

                    // Mock each story response
                    mockHttp.When("https://hacker-news.firebaseio.com/v0/item/1.json")
                            .Respond("application/json", JsonSerializer.Serialize(new Story
                            {
                                Title = "First Mock Story",
                                Url = "https://example.com/1",
                            }));

                    mockHttp.When("https://hacker-news.firebaseio.com/v0/item/2.json")
                            .Respond("application/json", JsonSerializer.Serialize(new Story
                            {
                                Title = "Second Mock Story",
                                Url = "https://example.com/2",
                            }));

                    var httpClient = mockHttp.ToHttpClient();
                    httpClient.BaseAddress = new Uri("https://hacker-news.firebaseio.com/");

                    // Add the StoryService with mocked HttpClient
                    services.AddScoped<IStoryService>(provider =>
                    {
                        var cache = provider.GetRequiredService<IMemoryCache>();
                        return new StoryService(httpClient, cache);
                    });
                });
            });
        }

        [Fact]
        public async Task GetStories_ReturnsMockedData()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/stories");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Contains("First Mock Story", json);
            Assert.Contains("Second Mock Story", json);
        }
    }
}
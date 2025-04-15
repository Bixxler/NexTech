using System;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using NexTech.API.Models;
using NexTech.API.Services;
using RichardSzalay.MockHttp;
namespace NextTech.IntegrationTests
{
    public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly IConfiguration _configuration;
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
                        return new StoryService(httpClient, cache, _configuration);
                    });
                });
            });
        }


        [Fact]
        public async Task GetStoriesEndpoint_ReturnsExpectedStories()
        {
            // Arrange
            var client = _factory.CreateClient();
            // Act
            var response = await client.GetAsync("/stories");
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var stories = JsonSerializer.Deserialize<List<Story>>(json);
            // Assert
            Assert.NotNull(stories);
            Assert.Equal(2, stories.Count);
            Assert.Contains(stories, s => s.Title == "First Mock Story" && s.Url == "https://example.com/1");
            Assert.Contains(stories, s => s.Title == "Second Mock Story" && s.Url == "https://example.com/2");
        }

        [Fact]
        public async Task GetStoriesAsync_ThrowsException_WhenApiFailsCompletely()
        {
            // Arrange
            var mockHttp = new MockHttpMessageHandler();

            // This expects the full URL
            mockHttp.When("https://hacker-news.firebaseio.com/v0/newstories.json")
                    .Throw(new HttpRequestException("API failure"));

            // Create a HttpClient with the mock handler and set BaseAddress
            var httpClient = new HttpClient(mockHttp)
            {
                BaseAddress = new Uri("https://hacker-news.firebaseio.com/")
            };

            // Create a MemoryCache that is initially empty
            var memoryCache = new MemoryCache(new MemoryCacheOptions());

            // Instantiate your service with the mocked HttpClient and MemoryCache
            var storyService = new StoryService(httpClient, memoryCache, _configuration);

            // Act & Assert: Since your service wraps exceptions, we check for ApplicationException.
            var exception = await Assert.ThrowsAsync<ApplicationException>(async () => await storyService.Get());
            Assert.Contains("Error while fetching stories from the API", exception.Message);
        }

    }
}
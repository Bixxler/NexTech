using System.Text.Json;
using RichardSzalay.MockHttp;
using NexTech.API.Models;
using NexTech.API.Services;
using System.Net;

namespace NextTech.Tests
{
    public class IntegrationTests
    {
        [Fact]
        public async Task GetStoriesAsync_ContinuesWhenSomeStoriesFail()
        {
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When("https://hacker-news.firebaseio.com/v0/newstories.json")
                    .Respond("application/json", "[1,2]");

            // Story 1 is successful
            mockHttp.When("https://hacker-news.firebaseio.com/v0/item/1.json")
                    .Respond("application/json", JsonSerializer.Serialize(new Story
                    {
                        Title = "Valid Story",
                        Url = "https://example.com/1"
                    }));

            // Story 2 fails with 404
            mockHttp.When("https://hacker-news.firebaseio.com/v0/item/2.json")
                    .Respond(HttpStatusCode.NotFound);

            var httpClient = new HttpClient(mockHttp)
            {
                BaseAddress = new Uri("https://hacker-news.firebaseio.com/")
            };

            var storyService = new StoryService(httpClient);

            // Act
            var result = await storyService.Get();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Valid Story", result[0].Title);
        }

        [Fact]
        public async Task GetStoriesAsync_UsesCache_OnSubsequentCalls()
        {
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When("https://hacker-news.firebaseio.com/v0/newstories.json")
                    .Respond("application/json", "[1]");

            mockHttp.When("https://hacker-news.firebaseio.com/v0/item/1.json")
                    .Respond("application/json", JsonSerializer.Serialize(new Story
                    {
                        Title = "Cached Story",
                        Url = "https://example.com"
                    }));

            var httpClient = new HttpClient(mockHttp)
            {
                BaseAddress = new Uri("https://hacker-news.firebaseio.com/")
            };

            var storyService = new StoryService(httpClient);

            // First call - populates the cache
            var firstCall = await storyService.Get();

            // Second call - should use cache
            var secondCall = await storyService.Get();

            // Assert
            Assert.Equal("Cached Story", secondCall[0].Title);
            Assert.Equal("https://example.com", secondCall[0].Url);
        }

        [Fact]
        public async Task GetStoriesAsync_ReturnsExpectedStories_WhenApiIsSuccessful()
        {
            var mockHttp = new MockHttpMessageHandler();

            mockHttp.When("https://hacker-news.firebaseio.com/v0/newstories.json")
                    .Respond("application/json", "[1,2]");

            mockHttp.When("https://hacker-news.firebaseio.com/v0/item/1.json")
                    .Respond("application/json", JsonSerializer.Serialize(new Story
                    {
                        Title = "Mock Story 1",
                        Url = "https://example.com/1"
                    }));

            mockHttp.When("https://hacker-news.firebaseio.com/v0/item/2.json")
                    .Respond("application/json", JsonSerializer.Serialize(new Story
                    {
                        Title = "Mock Story 2",
                        Url = "https://example.com/2"
                    }));

            var httpClient = new HttpClient(mockHttp)
            {
                BaseAddress = new Uri("https://hacker-news.firebaseio.com/")
            };

            var storyService = new StoryService(httpClient);

            // Act
            var stories = await storyService.Get();

            // Assert
            Assert.NotNull(stories);
            Assert.Equal(2, stories.Count);
            Assert.Contains(stories, s => s.Title == "Mock Story 1");
            Assert.Contains(stories, s => s.Title == "Mock Story 2");
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

            // Instantiate service with the mocked HttpClient and MemoryCache
            var storyService = new StoryService(httpClient);

            // Act & Assert: Since service wraps exceptions, we check for ApplicationException.
            var exception = await Assert.ThrowsAsync<ApplicationException>(async () => await storyService.Get());
            Assert.Contains("Error while fetching stories from the API", exception.Message);
        }

    }
}
using Microsoft.Extensions.Caching.Memory;
using Moq.Protected;
using Moq;
using NextTech.Server.Models;
using NextTech.Server.Services;
using System;
using System.Text.Json;
using System.Net;

namespace NextTech.Tests
{
    public class StoryServiceTests
    {
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly HttpClient _httpClient;
        private readonly StoryService _storyService;

        public StoryServiceTests()
        {
            // Setup Mock for HttpMessageHandler
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var memoryCache = new MemoryCache(new MemoryCacheOptions());

            // Create HttpClient with the mock handler
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://hacker-news.firebaseio.com/")
            };

            // Initialize StoryService with mocked HttpClient and in-memory cache
            _storyService = new StoryService(_httpClient, memoryCache);
        }

        [Fact]
        public async Task GetStoriesAsync_ReturnsMockedData()
        {
            // Arrange
            var mockedStoryIds = new List<int> { 1, 2 };
            var mockedStory1 = new Story { Title = "First Mock Story", Url = "https://example.com/1" };
            var mockedStory2 = new Story { Title = "Second Mock Story", Url = "https://example.com/2" };

            // Setup Mock HttpMessageHandler to return mocked responses
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync((HttpRequestMessage request, CancellationToken cancellationToken) =>
                {
                    if (request.RequestUri.ToString().Contains("newstories.json"))
                    {
                        var storyIdsJson = JsonSerializer.Serialize(mockedStoryIds);
                        return new HttpResponseMessage(HttpStatusCode.OK)
                        {
                            Content = new StringContent(storyIdsJson)
                        };
                    }
                    else if (request.RequestUri.ToString().Contains("item/1.json"))
                    {
                        var storyJson = JsonSerializer.Serialize(mockedStory1);
                        return new HttpResponseMessage(HttpStatusCode.OK)
                        {
                            Content = new StringContent(storyJson)
                        };
                    }
                    else if (request.RequestUri.ToString().Contains("item/2.json"))
                    {
                        var storyJson = JsonSerializer.Serialize(mockedStory2);
                        return new HttpResponseMessage(HttpStatusCode.OK)
                        {
                            Content = new StringContent(storyJson)
                        };
                    }

                    return new HttpResponseMessage(HttpStatusCode.NotFound);
                });

            // Act
            var result = await _storyService.Get();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            Assert.Contains(result, story => story.Title == "First Mock Story");
            Assert.Contains(result, story => story.Title == "Second Mock Story");
        }

        [Fact]
        public async Task GetStoriesAsync_ReturnsEmpty_WhenNoStoryIds()
        {
            // Arrange
            var mockedStoryIds = new List<int>();

            // Setup Mock HttpMessageHandler to return an empty list of story IDs
            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync((HttpRequestMessage request, CancellationToken cancellationToken) =>
                {
                    if (request.RequestUri.ToString().Contains("newstories.json"))
                    {
                        var storyIdsJson = JsonSerializer.Serialize(mockedStoryIds);
                        return new HttpResponseMessage(HttpStatusCode.OK)
                        {
                            Content = new StringContent(storyIdsJson)
                        };
                    }

                    return new HttpResponseMessage(HttpStatusCode.NotFound);
                });

            // Act
            var result = await _storyService.Get();

            // Assert
            Assert.Empty(result);  // Should return an empty list when no story IDs
        }



    }
}

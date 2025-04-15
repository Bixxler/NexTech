using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using System.Text.RegularExpressions;
using NextTech.Server.Models;

namespace NextTech.Server.Services
{
    public interface IStoryService
    {
        Task<List<Story>> Get();
    }

    public class StoryService(HttpClient httpClient, IMemoryCache cache) : IStoryService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly IMemoryCache _cache = cache;

        public async Task<List<Story>> Get()
        {
            try
            {

                // Try to get from cache
                if (_cache.TryGetValue("latestStories", out List<Story> cachedStories))
                {
                    if (cachedStories != null && cachedStories.Count != 0)
                        return cachedStories;
                }

                // Otherwise fetch
                var newStoriesUrl = "v0/newstories.json";
                var idsResponse = await _httpClient.GetStringAsync(newStoriesUrl);

                var ids = JsonSerializer.Deserialize<List<int>>(idsResponse);

                if (ids == null || !ids.Any())
                    return new List<Story>();

                var tasks = ids.Select(id => GetStoryById(id));

                var stories = await Task.WhenAll(tasks);

                var validStories = stories
                    .Where(story => story != null && !string.IsNullOrEmpty(story.Url))
                    .OrderBy(story =>
                    {
                        if (string.IsNullOrEmpty(story.Title))
                            return string.Empty;

                        //remove all the non letters from the sort
                        return Regex.Replace(story.Title, "[^a-zA-Z]", "").ToLower();
                    })
                    .ToList();

                // Cache it
                _cache.Set("latestStories", validStories, TimeSpan.FromMinutes(1)); // cache for 1 minute

                return validStories;

            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while fetching the stories: {ex.Message}");
            }
        }


        private async Task<Story> GetStoryById(int id)
        {
            var storyUrl = $"v0/item/{id}.json";
            var response = await _httpClient.GetAsync(storyUrl);

            if (!response.IsSuccessStatusCode)
                return null;

            var storyResponse = await response.Content.ReadAsStringAsync();
            var story = JsonSerializer.Deserialize<Story>(storyResponse);

            return story;
        }
    }
}

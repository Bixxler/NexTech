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
                        return cachedStories ?? [];
                }

                // Otherwise fetch
                var newStoriesUrl = "v0/newstories.json";
                var idsResponse = await _httpClient.GetStringAsync(newStoriesUrl);

                //deserialize the ids
                var ids = JsonSerializer.Deserialize<List<int>>(idsResponse);

                // Check if ids are null or empty
                if (ids == null || ids.Count == 0)
                    return [];

                // perhaps we should limit the number of stories to fetch
                var tasks = ids.Select(id => GetStoryById(id));

                // Fetch all stories in parallel
                var stories = await Task.WhenAll(tasks);

                // Check if stories are null or empty
                var validStories = stories
                    .Where(story => story != null && !string.IsNullOrEmpty(story.Url))
                    .OrderBy(story =>
                    {
                        if (string.IsNullOrEmpty(story.Title))
                            return string.Empty;

                        //remove all the non letters from the sort, this prevents titles like "second story" from being sorted before anything else
                        return Regex.Replace(story.Title, "[^a-zA-Z]", "").ToLower();
                    })
                    .ToList();

                // Cache it
                _cache.Set("latestStories", validStories, TimeSpan.FromMinutes(1)); // cache for 1 minute

                return validStories ?? [];

            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while fetching the stories: {ex.Message}");
            }
        }


        private async Task<Story> GetStoryById(int id)
        {
            try
            {
                var storyUrl = $"v0/item/{id}.json";
                var response = await _httpClient.GetAsync(storyUrl);

                //it's okay if these return null because we filter out null stories later
                if (!response.IsSuccessStatusCode)
                    return null;

                var storyResponse = await response.Content.ReadAsStringAsync();
                var story = JsonSerializer.Deserialize<Story>(storyResponse);

                return story;
            }
            catch(Exception ex)
            {
                //log the error but don't throw it, we want to continue fetching the other stories
                Console.WriteLine($"An error occurred while fetching the story with id {id}: {ex.Message}");
                return null;
            }
        }
    }
}

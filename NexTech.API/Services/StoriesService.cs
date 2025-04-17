using System.Text.Json;
using NexTech.API.Models;

namespace NexTech.API.Services
{
    public interface IStoryService
    {
        Task<List<Story>> Get();
    }

    public class StoryService(HttpClient httpClient) : IStoryService
    {
        private readonly HttpClient _httpClient = httpClient;
        //private readonly IMemoryCache _cache = cache;
        private readonly string _newUrlPart = "v0/newstories.json";
        private readonly string _storyUrlPart = "v0/item/{id}.json";

        private static List<Story>? _cachedStories;
        private static DateTime _cacheTime;
        private static readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);
        private static Task? _refreshTask;

        //Stale-while-revalidate technique
        public async Task<List<Story>> Get()
        {
            try
            {
                // If cache is still fresh, return it
                if (_cachedStories != null && DateTime.UtcNow - _cacheTime < _cacheDuration)
                {
                    return _cachedStories;
                }

                // If cache is stale but exists, return stale and refresh in background
                if (_cachedStories != null)
                {
                    // only one refresh task at a time
                    if (_refreshTask == null || _refreshTask.IsCompleted)
                    {
                        _refreshTask = Task.Run(async () =>
                        {
                            var fresh = await FetchStoriesAsync();
                            if (fresh != null && fresh.Count > 0)
                            {
                                _cachedStories = fresh;
                                _cacheTime = DateTime.UtcNow;
                            }
                        });
                    }

                    // Serve stale cache immediately
                    return _cachedStories;
                }

                // If no cache at all, do a full fetch and return
                _cachedStories = await FetchStoriesAsync();
                _cacheTime = DateTime.UtcNow;
                return _cachedStories;
            }

            catch(Exception ex)
            {
                throw new ApplicationException("Error while fetching stories from the API.", ex);
            }

        }

        public async Task<List<Story>> FetchStoriesAsync()
        {
            try
            {
                //doing pagination here would probably be worth it if there where thousands of results. But the current result set is around 500 so the paging is much faster if kept on client side

                // no longer using memory cache since it does not work with azure functions
                //if (_cache.TryGetValue("cachedStories", out List<Story>? cachedStories))
                //{
                //    if(cachedStories != null && cachedStories.Count > 0)
                //    {
                //        return cachedStories;
                //    }
                //}

                //var idsResponse = await _httpClient.GetStringAsync(_newUrlPart);
                var response = await _httpClient.GetAsync(_newUrlPart);
                //deserialize the ids
                await using var stream = await response.Content.ReadAsStreamAsync();


                var ids = JsonSerializer.Deserialize<List<int>>(stream);

                //var ids = await JsonSerializer.DeserializeAsync<List<int>>(idsResponse);
                // Check if ids are null or empty
                if (ids == null || ids.Count == 0)
                    return [];

                // perhaps we should limit the number of stories to fetch
                var tasks = ids.Select(id => GetStoryById(id));

                // Fetch all stories in parallel
                var stories = await Task.WhenAll(tasks);

                // was doing it this way but it's very inefficient having to Regex.Replace every loop
                //var validStories = stories
                //    .Where(story => story != null && !string.IsNullOrEmpty(story.Url) && !string.IsNullOrEmpty(story.Title))
                //    .OrderBy(story =>
                //    {
                //        //remove all the non letters from the sort, this prevents titles like "second story" from being sorted before anything else
                //        return Regex.Replace(story.Title, "[^a-zA-Z]", "").ToLower();
                //    })
                //    .ToList();

                var validStories = stories
                    .Where(story => story != null && !string.IsNullOrEmpty(story.Url) && !string.IsNullOrEmpty(story.Title)).OrderBy(story => story.Title)
                    //.Select(story => new
                    //{
                    //    Story = story,
                    //    //store the cleaned titles in CleanTitle var then order by that,
                    //    //remove all the non letters from the sort,
                    //    //this prevents titles like "second story" from being sorted before anything else
                    //    //CleanTitle = new string(story.Title.Where(char.IsLetter).ToArray()).ToLower()
                    //})
                    //.OrderBy(x => x.Story.Title)
                    //.Select(x => x.Story)
                    .ToList();

                // Cache it
                //_cache.Set("cachedStories", validStories, TimeSpan.FromMinutes(5)); // cache for 5 minutes?

                return validStories ?? [];

            }
            catch (JsonException ex)
            {
                throw new ApplicationException($"An unexpected error occurred while fetching the stories: {ex.Message}", ex);
            }
            catch (HttpRequestException ex)
            {
                // Log the exception or rethrow with more context if needed
                throw new ApplicationException("Error while fetching stories from the API.", ex);
            }
            catch (Exception ex)
            {
                // Log and throw with the message for unexpected errors
                throw new ApplicationException($"An unexpected error occurred while fetching the stories: {ex.Message}", ex);
            }
        }

        private async Task<Story> GetStoryById(int id)
        {
            try
            {
                var url = _storyUrlPart.Replace("{id}", id.ToString());
                var response = await _httpClient.GetAsync(url);
                //it's okay if these return null because we filter out null stories later
                if (!response.IsSuccessStatusCode)
                    return null;

                await using var stream = await response.Content.ReadAsStreamAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var story = await JsonSerializer.DeserializeAsync<Story>(stream);

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

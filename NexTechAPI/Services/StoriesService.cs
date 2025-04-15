using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using System.Text.RegularExpressions;
using NexTech.API.Models;
using System.Net.NetworkInformation;
using System.Data.Common;
using System;
using Microsoft.Extensions.Configuration;

namespace NexTech.API.Services
{
    public interface IStoryService
    {
        Task<List<Story>> Get();
    }

    public class StoryService(HttpClient httpClient, IMemoryCache cache, IConfiguration configuration) : IStoryService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly IMemoryCache _cache = cache;
        private readonly IConfiguration _configuration = configuration;
        private readonly string _newUrlPart = configuration["HackerNews:NewStoriesUrl"];
        private readonly string _storyUrlPart = configuration["HackerNews:StoryUrl"];

        public async Task<List<Story>> Get()
        {
            try
            {
                //doing pagination here would probably be worth it if there where thousands of results. But the current result set is around 500 so the paging is much faster if kept on client side

                // Try to get from cache
                if (_cache.TryGetValue("cachedStories", out List<Story>? cachedStories))
                {
                    if(cachedStories != null && cachedStories.Count > 0)
                    {
                        return cachedStories;
                    }
                }

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
                    .Where(story => story != null && !string.IsNullOrEmpty(story.Url) && !string.IsNullOrEmpty(story.Title))
                    .Select(story => new
                    {
                        Story = story,
                        //store the cleaned titles in CleanTitle var then order by that,
                        //remove all the non letters from the sort,
                        //this prevents titles like "second story" from being sorted before anything else
                        CleanTitle = new string(story.Title.Where(char.IsLetter).ToArray()).ToLower()
                    })
                    .OrderBy(x => x.CleanTitle)
                    .Select(x => x.Story)
                    .ToList();

                // Cache it
                _cache.Set("cachedStories", validStories, TimeSpan.FromMinutes(5)); // cache for 5 minutes?

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

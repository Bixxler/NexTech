using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using NextTech.Data;

namespace NextTech.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StoriesController : ControllerBase
{

    private readonly ILogger<StoriesController> _logger;

    public StoriesController(ILogger<StoriesController> logger)
    {
        _logger = logger;
    }

    private static HttpClient sharedClient = new()
    {
        BaseAddress = new Uri("https://hacker-news.firebaseio.com/v0/"),
        //https://hacker-news.firebaseio.com/v0/newstories.json
        //https://hacker-news.firebaseio.com/v0/item/{storyId}.json

    };

    [HttpGet("NewStories")]
    public async Task<IActionResult> GetNewStories()
    {
        var newStoriesUrl = "https://hacker-news.firebaseio.com/v0/newstories.json";
        var idsResponse = await sharedClient.GetStringAsync(newStoriesUrl);

        var ids = JsonSerializer.Deserialize<List<int>>(idsResponse);

        if (ids == null || !ids.Any())
            return NoContent();

        var tasks = ids.Take(50) // limit how many you load at once (optional)
            .Select(id => GetStoryById(id));

        var stories = await Task.WhenAll(tasks);

        var validStories = stories
            .Where(story => story != null && !string.IsNullOrEmpty(story.Url))
            .ToList();

        return Ok(validStories);
    }


    [HttpGet("GetById")]
    public async Task<Story> GetStoryById(int id)
    {
        try
        {
            var storyUrl = $"https://hacker-news.firebaseio.com/v0/item/{id}.json";
            var response = await sharedClient.GetAsync(storyUrl);

            if (!response.IsSuccessStatusCode)
                return null;

            var storyResponse = await response.Content.ReadAsStringAsync();
            var story = JsonSerializer.Deserialize<Story>(storyResponse);

            if (story == null || story.Type != "story" || string.IsNullOrEmpty(story.Url))
                return null;

            return story;
        }
        catch
        {
            return null;
        }
    }
}

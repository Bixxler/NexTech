using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using NextTech;
using System.Reflection.Metadata.Ecma335;
using NextTech.Server.Services;

namespace NextTech.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class StoriesController : ControllerBase
{

    private readonly ILogger<StoriesController> _logger;
    private readonly IStoryService _storyService;

    public StoriesController(ILogger<StoriesController> logger, IStoryService storyService)
    {
        _storyService = storyService;
        _logger = logger;
    }


    [HttpGet(Name = "GetStories")]
    public async Task<IActionResult> Get()
    {

        var stories = await _storyService.Get();

        if(stories == null)
        {
            return NotFound();
        }

        return Ok(stories);
    }
}

using Microsoft.AspNetCore.Mvc;
using NextTech.Server.Services;

namespace NextTech.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class StoriesController : ControllerBase
{

    private readonly IStoryService _storyService;

    public StoriesController(IStoryService storyService)
    {
        _storyService = storyService;
    }


    [HttpGet(Name = "GetStories")]
    public async Task<IActionResult> Get()
    {
        var stories = await _storyService.Get();

        if (stories == null)
        {
            return NotFound();
        }

        return Ok(stories);
    }
}

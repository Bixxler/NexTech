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
        try
        {
            var stories = await _storyService.Get();

            if (stories == null || !stories.Any())
            {
                return NotFound();
            }

            return Ok(stories);
        }
        catch (ApplicationException ex)
        {
            // Return a 500 status code with the error message
            return StatusCode(500, ex.Message);
        }
        catch (Exception ex)
        {
            // Return a generic 500 status code for unexpected errors
            return StatusCode(500, "An unexpected error occurred.");
        }
    }
}

using HackerNews.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace HackerNews.Api.Controllers;

[ApiController]
[Route("api/stories")]
public class StoriesController : ControllerBase
{
    private readonly IHackerNewsService _hackerNewsService;

    public StoriesController(IHackerNewsService hackerNewsService)
    {
        _hackerNewsService = hackerNewsService;
    }

    [HttpGet("best/{n:int}")]
    public async Task<IActionResult> GetBest(int n)
    {
        if (n <= 0)
            return BadRequest("n must be greater than 0.");

        var stories = await _hackerNewsService.GetBestStoriesAsync(n);

        return Ok(stories);
    }
}

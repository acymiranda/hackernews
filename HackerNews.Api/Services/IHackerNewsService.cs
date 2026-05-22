using HackerNews.Api.Models;

namespace HackerNews.Api.Services;

public interface IHackerNewsService
{
    Task<IEnumerable<StoryResponse>> GetBestStoriesAsync(int count);
}

namespace HackerNews.Api.Models;

public static class HackerNewsItemExtensions
{
    public static StoryResponse ToStoryResponse(this HackerNewsItem item) => new()
    {
        Title = item.Title,
        Uri = item.Url,
        PostedBy = item.By,
        Time = DateTimeOffset.FromUnixTimeSeconds(item.Time),
        Score = item.Score,
        CommentCount = item.Descendants
    };
}

using System.Text.Json;
using HackerNews.Api.Models;
using Microsoft.Extensions.Caching.Memory;

namespace HackerNews.Api.Services;

public class HackerNewsService : IHackerNewsService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public HackerNewsService(HttpClient httpClient, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _cache = cache;
    }

    public async Task<IEnumerable<StoryResponse>> GetBestStoriesAsync(int count)
    {
        if (_cache.TryGetValue("beststories", out List<StoryResponse>? stories) && stories is not null)
        {
            return stories.Take(count);
        }
            
        var idsJson = await _httpClient.GetStringAsync("beststories.json");
        var ids = JsonSerializer.Deserialize<int[]>(idsJson) ?? [];

        var fetched = await Task.WhenAll(ids.Select(FetchStoryAsync));

        stories = fetched
            .OfType<StoryResponse>()
            .OrderByDescending(s => s.Score)
            .ToList();

        _cache.Set("beststories", stories, TimeSpan.FromMinutes(5));

        return stories.Take(count);
    }

    private async Task<StoryResponse?> FetchStoryAsync(int id)
    {
        var json = await _httpClient.GetStringAsync($"item/{id}.json");
        var item = JsonSerializer.Deserialize<HackerNewsItem>(json, JsonOptions);

        return item?.ToStoryResponse();
    }
}

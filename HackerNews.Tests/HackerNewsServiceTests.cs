using System.Net;
using System.Text.Json;
using HackerNews.Api.Models;
using HackerNews.Api.Services;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Moq.Protected;

namespace HackerNews.Tests;

public class HackerNewsServiceTests
{
    private static HackerNewsService CreateService(HttpMessageHandler handler, IMemoryCache? cache = null)
    {
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://hacker-news.firebaseio.com/v0/")
        };

        cache ??= new MemoryCache(new MemoryCacheOptions());
        return new HackerNewsService(httpClient, cache);
    }

    [Fact]
    public async Task GetBestStoriesAsync_ReturnsSortedByScoreDescending()
    {
        // Arrange
        var ids = new[] { 1, 2 };
        var items = new Dictionary<int, HackerNewsItem>
        {
            [1] = new() { Id = 1, Title = "Story A", By = "userA", Score = 100, Time = 1570887781, Descendants = 20 },
            [2] = new() { Id = 2, Title = "Story B", By = "userB", Score = 200, Time = 1570887781, Descendants = 10 }
        };

        var (handler, _) = BuildMockHandler(ids, items);
        var service = CreateService(handler);

        // Act
        var result = (await service.GetBestStoriesAsync(2)).ToList();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(200, result[0].Score);
        Assert.Equal(100, result[1].Score);
    }

    [Fact]
    public async Task GetBestStoriesAsync_MapsFieldsCorrectly()
    {
        // Arrange
        var ids = new[] { 42 };
        var item = new HackerNewsItem
        {
            Id = 42,
            Title = "Test Story",
            Url = "https://example.com",
            By = "testuser",
            Score = 500,
            Time = 1570887781,
            Descendants = 99
        };

        var (handler, _) = BuildMockHandler(ids, new Dictionary<int, HackerNewsItem> { [42] = item });
        var service = CreateService(handler);

        // Act
        var result = (await service.GetBestStoriesAsync(1)).Single();

        // Assert
        Assert.Equal("Test Story", result.Title);
        Assert.Equal("https://example.com", result.Uri);
        Assert.Equal("testuser", result.PostedBy);
        Assert.Equal(500, result.Score);
        Assert.Equal(99, result.CommentCount);
        Assert.Equal(DateTimeOffset.FromUnixTimeSeconds(1570887781), result.Time);
    }

    [Fact]
    public async Task GetBestStoriesAsync_UsesCacheOnSecondCall()
    {
        // Arrange
        var ids = new[] { 1 };
        var item = new HackerNewsItem { Id = 1, Title = "Cached Story", By = "user", Score = 10, Time = 1570887781, Descendants = 1 };

        var (handler, mockHandler) = BuildMockHandler(ids, new Dictionary<int, HackerNewsItem> { [1] = item });
        var cache = new MemoryCache(new MemoryCacheOptions());
        var service = CreateService(handler, cache);

        // Act
        await service.GetBestStoriesAsync(1);
        await service.GetBestStoriesAsync(1);

        // Assert
        mockHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.AbsolutePath.Contains("beststories")),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    private static (HttpMessageHandler Handler, Mock<HttpMessageHandler> Mock) BuildMockHandler(
        int[] ids, Dictionary<int, HackerNewsItem> items)
    {
        var mock = new Mock<HttpMessageHandler>();

        mock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.AbsolutePath.Contains("beststories")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(ids))
            });

        foreach (var (id, item) in items)
        {
            mock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(r => r.RequestUri!.AbsolutePath.Contains($"item/{id}")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonSerializer.Serialize(item))
                });
        }

        return (mock.Object, mock);
    }
}

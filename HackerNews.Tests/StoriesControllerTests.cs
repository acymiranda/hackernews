using HackerNews.Api.Controllers;
using HackerNews.Api.Models;
using HackerNews.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace HackerNews.Tests;

public class StoriesControllerTests
{
    private readonly Mock<IHackerNewsService> _serviceMock = new();

    [Fact]
    public async Task GetBest_ReturnsOkWithStories()
    {
        // Arrange
        var stories = new List<StoryResponse>
        {
            new() { Title = "Story 1", Score = 100, PostedBy = "user1", Time = DateTimeOffset.UtcNow, CommentCount = 5 },
            new() { Title = "Story 2", Score = 50, PostedBy = "user2", Time = DateTimeOffset.UtcNow, CommentCount = 3 }
        };

        _serviceMock.Setup(s => s.GetBestStoriesAsync(2)).ReturnsAsync(stories);

        var controller = new StoriesController(_serviceMock.Object);

        // Act
        var result = await controller.GetBest(2);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(stories, ok.Value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task GetBest_InvalidN_ReturnsBadRequest(int n)
    {
        // Arrange
        var controller = new StoriesController(_serviceMock.Object);

        // Act
        var result = await controller.GetBest(n);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
        _serviceMock.Verify(s => s.GetBestStoriesAsync(It.IsAny<int>()), Times.Never);
    }
}

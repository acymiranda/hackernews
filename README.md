# HackerNews Best Stories API

A RESTful API built with ASP.NET Core 8 that retrieves the top *n* stories from the [Hacker News API](https://github.com/HackerNews/API), sorted by score in descending order.

## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

## Running the API

```bash
cd HackerNews.Api
dotnet run
```

The API will start at `https://localhost:5001` (or `http://localhost:5000`). Swagger UI is available at `/swagger` for interactive testing.

## Endpoint

**GET** `/api/stories/best/{n}`

Returns the top *n* stories by score.

**Example:**

```
GET /api/stories/best/10
```

**Response:**

```json
[
  {
    "title": "A uBlock Origin update was rejected from the Chrome Web Store",
    "uri": "https://github.com/uBlockOrigin/uBlock-issues/issues/745",
    "postedBy": "ismaildonmez",
    "time": "2019-10-12T13:43:01+00:00",
    "score": 1716,
    "commentCount": 572
  }
]
```

## Running the Tests

```bash
cd HackerNews.Tests
dotnet test
```

## Implementation Notes

**Why .NET 8?**  
.NET 8 is the current Long-Term Support (LTS) release, with support until November 2026. It was chosen over .NET 9 (a Standard-Term Support release with only 18 months of support) because LTS releases are generally preferred in enterprise environments where predictability and stability of the support cycle matter.

**Caching strategy**  
To avoid overloading the Hacker News API under high request volume, the service fetches all stories on the first request, builds the sorted list, and caches the result in memory for 5 minutes (`IMemoryCache`). Subsequent requests within that window are served directly from cache, with no upstream calls.

## Assumptions

- Stories with a `null` URL (Ask HN, Show HN, etc.) are included in the response with a `null` `uri` field.
- The Hacker News `beststories` endpoint already returns IDs sorted by score, but the service re-sorts by score to guarantee spec compliance.
- No authentication or rate limiting is applied — this is a developer-facing API as described in the brief.

## What I Would Add With More Time

- **Distributed cache** (e.g., Redis) for horizontal scalability across multiple API instances.
- **Background refresh** — a hosted service that proactively refreshes the story cache, so cache misses never block a request.
- **Rate limiting** on the API itself to prevent abuse.
- **Structured logging** with correlation IDs per request.
- **Integration tests** covering the full HTTP pipeline using `WebApplicationFactory`.


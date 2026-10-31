using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using MyResearchDesk.Web.Data;
using MyResearchDesk.Web.Models;
using MyResearchDesk.Web.Services;

namespace MyResearchDesk.Tests;

[Collection("db")]
public class ArticleApiTests(AppFixture app) : IAsyncLifetime
{
    private readonly HttpClient _client = app.CreateClient();

    public Task InitializeAsync() => app.ResetAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private async Task<JsonElement> CreateAsync(object body)
    {
        var res = await _client.PostAsJsonAsync("/api/articles", body);
        Assert.Equal(HttpStatusCode.Created, res.StatusCode);
        return await res.Content.ReadFromJsonAsync<JsonElement>();
    }

    private async Task<string[]> TitlesAsync(string query = "")
    {
        var items = await _client.GetFromJsonAsync<JsonElement[]>("/api/articles" + query);
        return items!.Select(i => i.GetProperty("title").GetString()!).ToArray();
    }

    [Fact]
    public async Task Crud_roundtrip()
    {
        var created = await CreateAsync(TestData.Article("First", takeaway: "hello"));
        var id = created.GetProperty("id").GetGuid();
        Assert.Equal("ToRead", created.GetProperty("status").GetString());

        var put = await _client.PutAsJsonAsync($"/api/articles/{id}", TestData.Article("Renamed", status: "WorthActingOn"));
        Assert.Equal(HttpStatusCode.NoContent, put.StatusCode);

        var got = await _client.GetFromJsonAsync<JsonElement>($"/api/articles/{id}");
        Assert.Equal("Renamed", got.GetProperty("title").GetString());
        Assert.Equal("WorthActingOn", got.GetProperty("status").GetString());
        Assert.True(got.GetProperty("updatedAt").GetDateTime() >= got.GetProperty("createdAt").GetDateTime());

        Assert.Equal(HttpStatusCode.NoContent, (await _client.DeleteAsync($"/api/articles/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync($"/api/articles/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await _client.DeleteAsync($"/api/articles/{id}")).StatusCode);
    }

    [Fact]
    public async Task Create_rejects_invalid_input()
    {
        var bad = new { title = "", url = "nope", topic = "AI", takeaway = "t", status = 9 };
        var res = await _client.PostAsJsonAsync("/api/articles", bad);
        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        var body = await res.Content.ReadAsStringAsync();
        Assert.Contains("Title", body);
        Assert.Contains("Url", body);
        Assert.Contains("Status", body);
    }

    [Fact]
    public async Task List_is_newest_first()
    {
        await CreateAsync(TestData.Article("Old"));
        await Task.Delay(20);
        await CreateAsync(TestData.Article("New"));
        Assert.Equal(["New", "Old"], await TitlesAsync());
    }

    [Fact]
    public async Task Search_matches_partial_case_insensitive_across_fields()
    {
        await CreateAsync(TestData.Article("Zero Trust", topic: "Security", source: "Microsoft", takeaway: "verify"));
        await CreateAsync(TestData.Article("Other", topic: "Data", takeaway: "100% coverage"));

        Assert.Equal(["Zero Trust"], await TitlesAsync("?q=ZERO"));
        Assert.Equal(["Zero Trust"], await TitlesAsync("?q=secur"));
        Assert.Equal(["Zero Trust"], await TitlesAsync("?q=micro"));
        Assert.Equal(["Other"], await TitlesAsync("?q=coverage"));
        Assert.Equal(["Other"], await TitlesAsync("?q=100%25"));
        Assert.Empty(await TitlesAsync("?q=_"));
    }

    [Fact]
    public async Task Filters_by_status_topic_and_combination()
    {
        await CreateAsync(TestData.Article("A", topic: "AI", status: "ToRead"));
        await CreateAsync(TestData.Article("B", topic: "AI", status: "Read"));
        await CreateAsync(TestData.Article("C", topic: "Azure", status: "Read"));

        Assert.Equivalent(new[] { "B", "C" }, await TitlesAsync("?status=Read"));
        Assert.Equivalent(new[] { "A", "B" }, await TitlesAsync("?topic=AI"));
        Assert.Equal(["B"], await TitlesAsync("?topic=AI&status=Read"));
    }

    [Fact]
    public async Task Dashboard_counts_match_data()
    {
        await CreateAsync(TestData.Article("A", status: "ToRead"));
        await CreateAsync(TestData.Article("B", status: "ToRead"));
        await CreateAsync(TestData.Article("C", status: "Read"));
        await CreateAsync(TestData.Article("D", status: "WorthActingOn"));

        using var scope = app.Services.CreateScope();
        var summary = await scope.ServiceProvider.GetRequiredService<ArticleService>().GetSummaryAsync();
        Assert.Equal(new DashboardSummary(4, 2, 1, 1), summary);

        var html = await _client.GetStringAsync("/");
        Assert.Matches(@"display-5"">4</div>", html);
    }

    [Fact]
    public async Task Search_over_1000_articles_is_under_two_seconds()
    {
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var now = DateTime.UtcNow;
            db.Articles.AddRange(Enumerable.Range(0, 1000).Select(i => new Article
            {
                Id = Guid.NewGuid(), Title = $"Article {i}", Url = "https://example.com",
                Topic = i % 2 == 0 ? "AI" : "Azure", Source = "Blog",
                Takeaway = $"Key learning number {i} about architecture and cloud",
                Status = (ArticleStatus)(i % 3 + 1), CreatedAt = now.AddSeconds(i), UpdatedAt = now
            }));
            await db.SaveChangesAsync();
        }

        await _client.GetAsync("/Articles?Q=warmup");
        var sw = Stopwatch.StartNew();
        var res = await _client.GetAsync("/Articles?Q=learning%20number%2099&Status=1");
        sw.Stop();

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.True(sw.Elapsed < TimeSpan.FromSeconds(2), $"search took {sw.Elapsed}");
    }
}

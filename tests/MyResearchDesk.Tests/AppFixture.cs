using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyResearchDesk.Web.Data;
using Testcontainers.PostgreSql;

namespace MyResearchDesk.Tests;

public class AppFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _db = new PostgreSqlBuilder("postgres:17").Build();

    public async Task InitializeAsync() => await _db.StartAsync();

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _db.DisposeAsync();
    }

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder) =>
        builder.UseSetting("ConnectionStrings:Default", _db.GetConnectionString());

    public async Task ResetAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.ExecuteSqlRawAsync("DELETE FROM \"Articles\"");
    }
}

[CollectionDefinition("db")]
public class DbCollection : ICollectionFixture<AppFixture>;

public static class TestData
{
    public static object Article(string title, string topic = "AI", string status = "ToRead",
        string takeaway = "takeaway", string? source = null) =>
        new { title, url = "https://example.com", topic, source, status, takeaway };
}

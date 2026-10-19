using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TeamCalendar.Api.Data;
using TeamCalendar.Tests.Infrastructure;

namespace TeamCalendar.Tests;

public class HostingTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public HostingTests(ApiFactory factory) => _factory = factory;

    [Theory]
    [InlineData("/")]
    [InlineData("/some/client/route")]
    public async Task Client_route_serves_index_html(string path)
    {
        var res = await _factory.CreateClient().GetAsync(path);

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        Assert.Contains("<title>TeamCalendar</title>", await res.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Missing_static_file_is_404_not_the_client_fallback()
    {
        var res = await _factory.CreateClient().GetAsync("/missing.js");

        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    [Fact]
    public async Task Unknown_api_route_is_404_not_the_client_fallback()
    {
        var res = await _factory.CreateClient().GetAsync("/api/does-not-exist");

        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    [Fact]
    public async Task Startup_applies_all_migrations_to_a_fresh_database()
    {
        _ = _factory.CreateClient(); // starts the host, which migrates
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Assert.Empty(await db.Database.GetPendingMigrationsAsync());
        Assert.Contains(await db.Database.GetAppliedMigrationsAsync(), m => m.EndsWith("_Initial"));
    }
}

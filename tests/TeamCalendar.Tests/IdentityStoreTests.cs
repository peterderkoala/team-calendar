using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TeamCalendar.Api.Auth;
using TeamCalendar.Api.Data;
using TeamCalendar.Tests.Infrastructure;

namespace TeamCalendar.Tests;

// Test-only endpoints: deliberately not [ApiController], so only the cookie events decide 401/403.
[Route("api/probe")]
public class ProbeController : ControllerBase
{
    [HttpGet("secured")]
    public IActionResult Secured() => Ok();

    [HttpGet("admin")]
    [Authorize(Roles = Roles.Admin)]
    public IActionResult AdminOnly() => Ok();

    [HttpGet("open")]
    [AllowAnonymous]
    public IActionResult Open() => Ok();

    [HttpPost("login/{name}")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(string name, [FromServices] UserManager<AppUser> um, [FromServices] SignInManager<AppUser> sm)
    {
        await sm.SignInAsync((await um.FindByNameAsync(name))!, isPersistent: false);
        return Ok();
    }
}

public class IdentityStoreTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public IdentityStoreTests(ApiFactory factory) => _factory = factory;

    private WebApplicationFactoryWithProbe Probe() => new(_factory);

    private sealed class WebApplicationFactoryWithProbe(ApiFactory f)
    {
        public HttpClient Client { get; } = f.WithWebHostBuilder(b => b.ConfigureTestServices(s =>
            s.AddControllers().AddApplicationPart(typeof(ProbeController).Assembly))).CreateClient();

        public IServiceProvider Services { get; } = f.Services;
    }

    [Fact]
    public async Task User_round_trips_MustChangePassword_and_Language()
    {
        using var scope = _factory.Services.CreateScope();
        var um = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var created = await um.CreateAsync(
            new AppUser { UserName = "roundtrip", MustChangePassword = true, Language = "de" }, "Passw0rd!");
        Assert.True(created.Succeeded, string.Join(",", created.Errors.Select(e => e.Description)));

        var loaded = await um.FindByNameAsync("roundtrip");

        Assert.True(loaded!.MustChangePassword);
        Assert.Equal("de", loaded.Language);
    }

    [Fact]
    public async Task Admin_role_exists_after_migration()
    {
        using var scope = _factory.Services.CreateScope();

        Assert.True(await scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>().RoleExistsAsync(Roles.Admin));
    }

    [Fact]
    public async Task Data_protection_keys_are_stored_in_the_database_and_shared_between_instances()
    {
        var payload = _factory.Services.GetRequiredService<IDataProtectionProvider>()
            .CreateProtector("test").Protect("secret");

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            Assert.True(await db.DataProtectionKeys.CountAsync() >= 1);
        }

        using var second = _factory.WithWebHostBuilder(_ => { });
        var unprotected = second.Services.GetRequiredService<IDataProtectionProvider>()
            .CreateProtector("test").Unprotect(payload);

        Assert.Equal("secret", unprotected);
    }

    [Fact]
    public async Task Unauthenticated_api_request_is_401_without_redirect()
    {
        var res = await Probe().Client.GetAsync("/api/probe/secured");

        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
        Assert.Null(res.Headers.Location);
    }

    [Fact]
    public async Task Forbidden_api_request_is_403_without_redirect()
    {
        var p = Probe();
        using (var scope = p.Services.CreateScope())
        {
            var um = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            Assert.True((await um.CreateAsync(new AppUser { UserName = "member" }, "Passw0rd!")).Succeeded);
        }
        Assert.Equal(HttpStatusCode.OK, (await p.Client.PostAsync("/api/probe/login/member", null)).StatusCode);

        var res = await p.Client.GetAsync("/api/probe/admin");

        Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
        Assert.Null(res.Headers.Location);
    }

    [Fact]
    public async Task Endpoints_require_authentication_unless_AllowAnonymous()
    {
        var client = Probe().Client;

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/probe/open")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/probe/secured")).StatusCode);
    }
}

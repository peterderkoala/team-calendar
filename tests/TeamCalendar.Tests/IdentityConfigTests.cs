using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TeamCalendar.Api.Data;
using TeamCalendar.Tests.Infrastructure;

namespace TeamCalendar.Tests;

public class IdentityConfigTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;

    public IdentityConfigTests(ApiFactory factory) => _factory = factory;

    [Fact]
    public void Model_types_match_the_contract()
    {
        Assert.True(typeof(IdentityUser).IsAssignableFrom(typeof(AppUser)));
        Assert.True(typeof(IdentityDbContext<AppUser>).IsAssignableFrom(typeof(AppDbContext)));
        Assert.True(typeof(IDataProtectionKeyContext).IsAssignableFrom(typeof(AppDbContext)));
    }

    [Fact]
    public async Task New_user_defaults_to_no_forced_change_and_no_language()
    {
        using var scope = _factory.Services.CreateScope();
        var um = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        Assert.True((await um.CreateAsync(new AppUser { UserName = "defaults" }, "Passw0rd!")).Succeeded);

        var loaded = await um.FindByNameAsync("defaults");

        Assert.False(loaded!.MustChangePassword);
        Assert.Null(loaded.Language);
    }

    [Fact]
    public void Lockout_uses_identity_defaults_and_stamp_is_validated_every_request()
    {
        var sp = _factory.Services;
        var lockout = sp.GetRequiredService<IOptions<IdentityOptions>>().Value.Lockout;

        Assert.Equal(5, lockout.MaxFailedAccessAttempts);
        Assert.True(lockout.AllowedForNewUsers);
        Assert.Equal(TimeSpan.FromMinutes(5), lockout.DefaultLockoutTimeSpan);
        Assert.Equal(TimeSpan.Zero, sp.GetRequiredService<IOptions<SecurityStampValidatorOptions>>().Value.ValidationInterval);
    }

    [Fact]
    public async Task Admin_role_seed_is_the_fixed_row()
    {
        using var scope = _factory.Services.CreateScope();
        var role = await scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>().FindByIdAsync("admin");

        Assert.Equal("admin", role!.Name);
    }
}

using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using TeamCalendar.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(o => o.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddControllers();
builder.Services.Configure<ForwardedHeadersOptions>(o =>
{
    o.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    o.KnownIPNetworks.Clear();
    o.KnownProxies.Clear();
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
}

app.UseForwardedHeaders();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.MapControllers();
app.MapFallbackToFile("/", "index.html"); // catch-all below does not match the empty path
app.MapFallbackToFile(@"{*path:nonfile:regex(^(?!api(/|$)).*)}", "index.html"); // /api/* stays a real 404

app.Run();

public partial class Program;

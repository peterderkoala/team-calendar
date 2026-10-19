using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Npgsql;
using Testcontainers.PostgreSql;

namespace TeamCalendar.Tests.Infrastructure;

/// <summary>Starts the app on a fresh, uniquely named database inside one shared Postgres container.</summary>
public class ApiFactory : WebApplicationFactory<Program>
{
    // One container per test run (reaped by Ryuk); each factory gets its own database.
    private static readonly Lazy<Task<PostgreSqlContainer>> Container = new(async () =>
    {
        var c = new PostgreSqlBuilder("postgres:17").Build();
        await c.StartAsync();
        return c;
    });

    public string ConnectionString { get; }

    public ApiFactory()
    {
        var admin = Container.Value.GetAwaiter().GetResult().GetConnectionString();
        var db = "test_" + Guid.NewGuid().ToString("N");
        using (var conn = new NpgsqlConnection(admin))
        {
            conn.Open();
            using var cmd = new NpgsqlCommand($"CREATE DATABASE \"{db}\"", conn);
            cmd.ExecuteNonQuery();
        }
        ConnectionString = new NpgsqlConnectionStringBuilder(admin) { Database = db }.ConnectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.UseSetting("ConnectionStrings:Default", ConnectionString);
}

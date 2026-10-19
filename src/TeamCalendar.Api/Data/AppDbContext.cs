using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace TeamCalendar.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<AppUser>(options), IDataProtectionKeyContext
{
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Fixed values, or every migration rewrites the row.
        builder.Entity<IdentityRole>().HasData(new IdentityRole
        {
            Id = "admin",
            Name = Auth.Roles.Admin,
            NormalizedName = "ADMIN",
            ConcurrencyStamp = "admin",
        });
    }
}

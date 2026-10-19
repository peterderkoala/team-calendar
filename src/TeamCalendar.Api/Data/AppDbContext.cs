using Microsoft.EntityFrameworkCore;

namespace TeamCalendar.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options);

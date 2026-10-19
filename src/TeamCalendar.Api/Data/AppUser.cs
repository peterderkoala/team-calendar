using Microsoft.AspNetCore.Identity;

namespace TeamCalendar.Api.Data;

public class AppUser : IdentityUser
{
    public bool MustChangePassword { get; set; }

    public string? Language { get; set; }
}

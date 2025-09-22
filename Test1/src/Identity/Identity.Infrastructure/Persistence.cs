using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure;
public class AppIdentityDb : IdentityDbContext<IdentityUser, IdentityRole, string>
{
    public AppIdentityDb(DbContextOptions<AppIdentityDb> options) : base(options) { }
}

using Microsoft.EntityFrameworkCore;
using Velora.Database.Entities;

namespace Velora.Database
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
    }
}

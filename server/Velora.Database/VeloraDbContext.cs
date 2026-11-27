using Microsoft.EntityFrameworkCore;
using Velora.Database.Entities;

namespace Velora.Database
{
    public class VeloraDbContext: DbContext
    {
        public VeloraDbContext(DbContextOptions<VeloraDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
    }
}

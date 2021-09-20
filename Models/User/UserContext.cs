using Microsoft.EntityFrameworkCore;

namespace MyCloud.Models.User
{
    public sealed class UserContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public UserContext(DbContextOptions<UserContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
    }
}
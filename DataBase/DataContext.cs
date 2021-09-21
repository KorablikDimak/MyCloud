using Microsoft.EntityFrameworkCore;
using MyCloud.Models.MyFile;

namespace MyCloud.DataBase
{
    public sealed class DataContext : DbContext
    {
        public DbSet<MyFileInfo> Files { get; set; }
        public DbSet<Models.User.User> Users { get; set; }

        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
    }
}
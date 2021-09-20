using Microsoft.EntityFrameworkCore;

namespace MyCloud.Models.MyFile
{
    public sealed class MyFileInfoContext : DbContext
    {
        public DbSet<MyFileInfo> Files { get; set; }

        public MyFileInfoContext(DbContextOptions<MyFileInfoContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
    }
}
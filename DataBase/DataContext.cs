using Microsoft.EntityFrameworkCore;
using MyCloud.Models.MyFile;
using MyCloud.Models.Registration;
using MyCloud.Models.User;

namespace MyCloud.DataBase
{
    public sealed class DataContext : DbContext
    {
        public DbSet<MyFileInfo> Files { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<PersonalityData> Personality { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<UserToConfirm> Emails { get; set; }

        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
            Database.EnsureCreated();
        }
    }
}
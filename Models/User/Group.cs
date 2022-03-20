using System.Collections.Generic;
using MyCloud.Models.Login;
using MyCloud.Models.MyFile;

namespace MyCloud.Models.User
{
    public class Group : GroupLogin
    {
        public int Id { get; set; }
        public ICollection<User> Users { get; set; }
        public ICollection<MyFileInfo> Files { get; set; }

        public Group()
        {
            Users = new List<User>();
            Files = new List<MyFileInfo>();
        }
    }
}
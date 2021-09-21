using System.Collections.Generic;
using MyCloud.Models.MyFile;

namespace MyCloud.Models.User
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public ICollection<MyFileInfo> Files { get; set; }
        public PersonalityData PersonalityData { get; set; }

        public User()
        {
            Files = new List<MyFileInfo>();
        }
    }
}
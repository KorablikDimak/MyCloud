using System.Collections.Generic;

namespace MyCloud.Models.User
{
    public class Group : GroupLogin
    {
        public int Id { get; set; }
        public ICollection<User> Users { get; set; }

        public Group()
        {
            Users = new List<User>();
        }
    }
}
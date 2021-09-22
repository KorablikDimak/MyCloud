using System.Collections.Generic;

namespace MyCloud.Models.User
{
    public class Group
    {
        public int Id { get; set; }
        public string GroupName { get; set; }
        public ICollection<User> Users { get; set; }

        public Group()
        {
            Users = new List<User>();
        }
    }
}
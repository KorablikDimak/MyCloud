using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MyCloud.Models.MyFile;

namespace MyCloud.Models.User
{
    public class User
    {
        public int Id { get; set; }
        [Required]
        [StringLength(20, MinimumLength = 3)]
        public string UserName { get; set; }
        [Required]
        [StringLength(32)]
        public string Password { get; set; }
        public ICollection<MyFileInfo> Files { get; set; }
        public PersonalityData PersonalityData { get; set; }
        public ICollection<Group> Groups { get; set; }
        public string IconName { get; set; }

        public User()
        {
            Files = new List<MyFileInfo>();
            Groups = new List<Group>();
        }
    }
}
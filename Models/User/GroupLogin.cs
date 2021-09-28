using System.ComponentModel.DataAnnotations;

namespace MyCloud.Models.User
{
    public class GroupLogin
    {
        [Required]
        [StringLength(20, MinimumLength = 3)]
        public string Name { get; set; }
        [Required]
        [StringLength(20, MinimumLength = 8)]
        public string GroupPassword { get; set; }
    }
}
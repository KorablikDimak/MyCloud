using System.ComponentModel.DataAnnotations;

namespace MyCloud.Models.Login
{
    public class GroupLogin
    {
        [Required]
        [StringLength(20, MinimumLength = 3)]
        public string Name { get; set; }
        [Required]
        [StringLength(32, MinimumLength = 8)]
        public string GroupPassword { get; set; }
    }
}
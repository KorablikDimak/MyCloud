using System.ComponentModel.DataAnnotations;

namespace MyCloud.Models.Login
{
    public class ChangePasswordModel
    {
        [Required]
        [StringLength(20, MinimumLength = 8)]
        public string OldPassword { get; set; }
        [Required]
        [StringLength(20, MinimumLength = 8)]
        public string NewPassword { get; set; }
    }
}
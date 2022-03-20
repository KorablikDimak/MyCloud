using System.ComponentModel.DataAnnotations;

namespace MyCloud.Models.Login
{
    public class LoginModel
    {
        [Required]
        [StringLength(20, MinimumLength = 3)]
        public string UserName { get; set; }
        [Required]
        [StringLength(32, MinimumLength = 8)]
        public string Password { get; set; }
    }
}
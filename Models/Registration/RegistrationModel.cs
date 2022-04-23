using System.ComponentModel.DataAnnotations;

namespace MyCloud.Models.Registration
{
    public class RegistrationModel
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        [StringLength(32, MinimumLength = 8)]
        public string Password { get; set; }
        [Compare("Password")]
        public string ConfirmPassword { get; set; } 
    }
}
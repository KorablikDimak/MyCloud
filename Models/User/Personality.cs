using System.ComponentModel.DataAnnotations;

namespace MyCloud.Models.User
{
    public class Personality
    {
        public virtual int Id { get; set; }
        [Required]
        [StringLength(20, MinimumLength = 3)]
        public string UserName { get; set; }
        public string Surname { get; set; }
        public string Name { get; set; }
    }
}
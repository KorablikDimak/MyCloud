using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyCloud.Models.User
{
    public class PersonalityData : Personality
    {
        [Key]
        [ForeignKey("User")]
        public override int Id { get; set; }
        public User User { get; set; }
    }
}
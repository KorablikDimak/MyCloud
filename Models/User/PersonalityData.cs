namespace MyCloud.Models.User
{
    public class PersonalityData
    {
        public string Surname { get; set; }
        public string Name { get; set; }
        public User User { get; set; }

        public PersonalityData()
        {
            Surname = "";
            Name = "";
        }
    }
}
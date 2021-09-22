namespace MyCloud.Models.User
{
    public class Personality
    {
        public virtual int Id { get; set; }
        public string UserName { get; set; } = "";
        public string Surname { get; set; } = "";
        public string Name { get; set; } = "";
    }
}
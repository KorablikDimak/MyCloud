using System;

namespace MyCloud.Models.MyFile
{
    public class MyFileInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string TypeOfFile { get; set; }
        public DateTime DateTime { get; set; }
        public long Size { get; set; }
        public User.User User { get; set; }
    }
}
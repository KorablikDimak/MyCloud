using System;

namespace MyCloud.Models.MyFile
{
    public class MyFileInfo
    {
        public string Name { get; set; }
        public string TypeOfFile { get; set; }
        public DateTime DateTimeUpload { get; set; }
        public long Size { get; set; }
    }
}
using System;

namespace MyCloud.Models.MyFile
{
    public class MyFileInfo
    {
        public string Name { get; set; }
        public string TypeOfFile { get; set; }
        public DateTime DateTimeUpload { get; set; }
        public long Size { get; set; }
        
        public MyFileInfo(){}
        public MyFileInfo(string name, string typeOfFile, DateTime dateTime, long size)
        {
            Name = name;
            TypeOfFile = typeOfFile;
            DateTimeUpload = dateTime;
            Size = size;
        }
    }
}
using System;
using System.Data.SqlTypes;

namespace MyCloud.Models.MyFile
{
    public class MyFileInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string TypeOfFile { get; set; }
        public SqlDateTime DateTimeUpload { get; set; }
        public long Size { get; set; }
        
        public MyFileInfo(){}
        public MyFileInfo(int id, string name, string typeOfFile, SqlDateTime dateTime, long size)
        {
            Id = id;
            Name = name;
            TypeOfFile = typeOfFile;
            DateTimeUpload = dateTime;
            Size = size;
        }
        public MyFileInfo(string name, string typeOfFile, DateTime dateTime, long size)
        {
            Name = name;
            TypeOfFile = typeOfFile;
            DateTimeUpload = new SqlDateTime(dateTime);
            Size = size;
        }
    }
}
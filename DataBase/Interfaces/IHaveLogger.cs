using InfoLog;

namespace MyCloud.DataBase.Interfaces
{
    public interface IHaveLogger
    {
        public ILogger Logger { get; set; }
    }
}
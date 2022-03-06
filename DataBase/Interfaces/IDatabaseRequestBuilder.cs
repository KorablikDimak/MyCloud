using MyCloud.DataBase.DataRequestBuilder;

namespace MyCloud.DataBase.Interfaces
{
    public interface IDatabaseRequestBuilder
    {
        public DatabaseRequest DatabaseRequest { get; }
        public DataContext Context { init; }
        public void CreateDatabaseRequest();
    }
}
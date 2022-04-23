using MyCloud.DataBase.DataRequestBuilder;

namespace MyCloud.DataBase.Interfaces
{
    public interface IRepositoryBuilder
    {
        public Repository Repository { get; }
        public DataContext Context { init; }
        public void CreateRepository();
    }
}
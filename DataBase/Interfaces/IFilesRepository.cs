using System.Linq;
using System.Threading.Tasks;
using MyCloud.Models.MyFile;
using MyCloud.Models.User;

namespace MyCloud.DataBase.Interfaces
{
    public interface IFilesRepository
    {
        public IQueryable<MyFileInfo> FindFiles<T>(T criterion);
        public Task<bool> AddFileAsync<T>(MyFileInfo fileInfo, T criterion);
        public Task<bool> DeleteFileAsync<T>(string fileName, T criterion);
        public Task<bool> DeleteAllFilesAsync<T>(T criterion);
    }
}
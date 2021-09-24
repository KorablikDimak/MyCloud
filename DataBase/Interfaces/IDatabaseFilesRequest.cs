using System.Linq;
using System.Threading.Tasks;
using MyCloud.Models.MyFile;
using MyCloud.Models.User;

namespace MyCloud.DataBase.Interfaces
{
    public interface IDatabaseFilesRequest
    {
        public IQueryable<MyFileInfo> FindFiles(string userName);
        public Task<bool> AddFileAsync(User user, MyFileInfo fileInfo);
        public Task<bool> DeleteFileAsync(string userName, string fileName);
        public Task<bool> DeleteAllFilesAsync(string userName);
    }
}
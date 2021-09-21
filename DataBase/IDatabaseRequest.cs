using System.Linq;
using System.Threading.Tasks;
using MyCloud.Models.MyFile;
using MyCloud.Models.User;

namespace MyCloud.DataBase
{
    public interface IDatabaseRequest
    {
        public Task<User> FindUserAsync(string userName, string password);
        public Task<bool> AddUserAsync(string userName, string password);
        public Task<bool> DeleteUserAsync(string userName, string password);
        public Task<bool> AddFileAsync(string userName, MyFileInfo fileInfo);
        public IQueryable<MyFileInfo> FindFiles(string userName);
        public Task<bool> DeleteFileAsync(string userName, string fileName);
        public Task<bool> DeleteAllFilesAsync(string userName);
    }
}
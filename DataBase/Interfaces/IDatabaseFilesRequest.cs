using System.Linq;
using System.Threading.Tasks;
using MyCloud.Models.MyFile;
using MyCloud.Models.User;

namespace MyCloud.DataBase.Interfaces
{
    public interface IDatabaseFilesRequest
    {
        public Group Group { set; }
        public User User { set; }
        
        public IQueryable<MyFileInfo> FindFiles();
        public Task<bool> AddFileAsync(MyFileInfo fileInfo);
        public Task<bool> DeleteFileAsync(string fileName);
        public Task<bool> DeleteAllFilesAsync();
    }
}
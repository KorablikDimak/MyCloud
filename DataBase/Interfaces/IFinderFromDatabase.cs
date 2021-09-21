using System.Linq;
using System.Threading.Tasks;
using MyCloud.Models.MyFile;
using MyCloud.Models.User;

namespace MyCloud.DataBase.Interfaces
{
    public interface IFinderFromDatabase
    {
        public Task<User> FindUserAsync(string userName, string password);
        public IQueryable<MyFileInfo> FindFiles(string userName);
    }
}
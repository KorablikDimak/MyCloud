using System.Threading.Tasks;
using MyCloud.Models.MyFile;

namespace MyCloud.DataBase.Interfaces
{
    public interface IAdderIntoDatabase
    {
        public Task<bool> AddFileAsync(string userName, MyFileInfo fileInfo);
        public Task<bool> AddUserAsync(string userName, string password);
    }
}
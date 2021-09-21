using System.Threading.Tasks;

namespace MyCloud.DataBase.Interfaces
{
    public interface IDeleterFromDatabase
    {
        public Task<bool> DeleteUserAsync(string userName, string password);
        public Task<bool> DeleteFileAsync(string userName, string fileName);
        public Task<bool> DeleteAllFilesAsync(string userName);
    }
}
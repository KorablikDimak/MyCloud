using System.Threading.Tasks;
using MyCloud.Models.User;

namespace MyCloud.DataBase.Interfaces
{
    public interface IDatabaseUsersRequest
    {
        public Task<User> FindUserAsync(string userName);
        public Task<User> FindUserAsync(string userName, string password);
        public Task<bool> AddUserAsync(string userName, string password);
        public Task<bool> DeleteUserAsync(string userName, string password);
        public Task<bool> ChangePasswordAsync(string userName, string oldPassword, string newPassword);
        public Task<bool> ChangeUserNameAsync(string oldUserName, string newUserName);
    }
}
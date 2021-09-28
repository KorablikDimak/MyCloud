using System.Collections.Generic;
using System.Threading.Tasks;
using MyCloud.Models.User;

namespace MyCloud.DataBase.Interfaces
{
    public interface IDatabaseUsersRequest
    {
        public Task<List<User>> FindUsersInGroup(GroupLogin groupLogin);
        public Task<User> FindUserAsync(string userName);
        public Task<User> FindUserAsync(string userName, string password);
        public Task<bool> AddUserAsync(string userName, string password);
        public Task<bool> DeleteUserAsync(string userName, string password);
        public Task<bool> ChangePasswordAsync(string userName, string oldPassword, string newPassword);
        public Task<bool> ChangeUserNameAsync(Personality personality, string newUserName);
        public Task<bool> SetIcon(string userName, string iconName);
        public Task<string> GetIcon(string userName);
    }
}
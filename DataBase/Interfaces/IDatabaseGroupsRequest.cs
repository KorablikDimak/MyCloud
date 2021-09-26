using System.Collections.Generic;
using System.Threading.Tasks;
using MyCloud.Models.User;

namespace MyCloud.DataBase.Interfaces
{
    public interface IDatabaseGroupsRequest
    {
        public Task<List<Group>> FindGroupsInUser(string userName);
        public Task<bool> CreateGroupAsync(GroupLogin groupLogin, User user);
        public Task<bool> DeleteGroupAsync(GroupLogin groupLogin);
        public Task<bool> ChangeGroupLoginAsync(GroupLogin groupLogin, GroupLogin newGroupLogin);
        public Task<bool> AddUserInGroupAsync(GroupLogin groupLogin, User user);
        public Task<bool> RemoveUserFromGroupAsync(GroupLogin groupLogin, User user);
    }
}
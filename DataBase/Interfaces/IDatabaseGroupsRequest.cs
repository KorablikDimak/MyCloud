using System.Collections.Generic;
using System.Threading.Tasks;
using MyCloud.Models.User;

namespace MyCloud.DataBase.Interfaces
{
    public interface IDatabaseGroupsRequest
    {
        public DataContext DatabaseContext { init; }
        Task<List<Group>> FindGroupsInUser(string userName);
        Task<bool> CreateGroupAsync(GroupLogin groupLogin, User user);
        Task<bool> DeleteGroupAsync(GroupLogin groupLogin);
        Task<bool> ChangeGroupLoginAsync(GroupLogin groupLogin, GroupLogin newGroupLogin);
        Task<bool> AddUserInGroupAsync(GroupLogin groupLogin, User user);
        Task<bool> RemoveUserFromGroupAsync(GroupLogin groupLogin, User user);
    }
}
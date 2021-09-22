using System.Threading.Tasks;
using MyCloud.Models.User;

namespace MyCloud.DataBase.Interfaces
{
    public interface IDatabaseGroupsRequest
    {
        Task<Group> FindGroup(string groupName);
        Task<Group> FindGroup(GroupLogin groupLogin);
        Task<bool> CreateGroup(GroupLogin groupLogin, User user);
        Task<bool> DeleteGroup(Group group);
        Task<bool> ChangeGroupName(Group group, string newGroupName);
        Task<bool> ChangeGroupPassword(Group group, string newGroupPassword);
        Task<bool> AddUserInGroup(User user, Group group);
        Task<bool> RemoveUserFromGroup(User user, Group group);
    }
}
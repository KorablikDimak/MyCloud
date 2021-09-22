using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyCloud.DataBase.Interfaces;
using MyCloud.Models.User;

namespace MyCloud.DataBase
{
    public class DatabaseGroupRequest : IDatabaseGroupsRequest
    {
        private readonly DataContext _databaseContext;

        public DatabaseGroupRequest(DataContext context)
        {
            _databaseContext = context;
        }

        public async Task<Group> FindGroup(string groupName)
        {
            return await _databaseContext.Groups.FirstOrDefaultAsync(group => 
                group.GroupName == groupName);
        }

        public async Task<Group> FindGroup(GroupLogin groupLogin)
        {
            return await _databaseContext.Groups.FirstOrDefaultAsync(group => 
                group.GroupName == groupLogin.GroupName &&
                group.GroupPassword == groupLogin.GroupPassword);
        }

        public async Task<bool> CreateGroup(GroupLogin groupLogin, User user)
        {
            try
            {
                var group = new Group
                {
                    GroupName = groupLogin.GroupName,
                    GroupPassword = groupLogin.GroupPassword
                };
                group.Users.Add(user);
                user.Groups.Add(group);
                await _databaseContext.Groups.AddAsync(group);
                await _databaseContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            return true;
        }

        public async Task<bool> DeleteGroup(Group group)
        {
            try
            {
                _databaseContext.Groups.Remove(group);
                await _databaseContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            return true;
        }

        public async Task<bool> ChangeGroupName(Group group, string newGroupName)
        {
            try
            {
                group.GroupName = newGroupName;
                await _databaseContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            return true;
        }

        public async Task<bool> ChangeGroupPassword(Group group, string newGroupPassword)
        {
            try
            {
                group.GroupPassword = newGroupPassword;
                await _databaseContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            return true;
        }

        public async Task<bool> AddUserInGroup(User user, Group group)
        {
            try
            {
                group.Users.Add(user);
                user.Groups.Add(group);
                await _databaseContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            return true;
        }

        public async Task<bool> RemoveUserFromGroup(User user, Group group)
        {
            try
            {
                group.Users.Remove(user);
                user.Groups.Remove(group);
                await _databaseContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            return true;
        }
    }
}
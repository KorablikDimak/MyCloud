using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyCloud.DataBase.Interfaces;
using MyCloud.Models.User;

namespace MyCloud.DataBase
{
    public class DatabaseGroupsRequest : IDatabaseGroupsRequest
    {
        private readonly DataContext _databaseContext;

        public DatabaseGroupsRequest(DataContext context)
        {
            _databaseContext = context;
        }

        private async Task<Group> FindGroupAsync(GroupLogin groupLogin)
        {
            return await _databaseContext.Groups.FirstOrDefaultAsync(group => 
                group.GroupName == groupLogin.GroupName &&
                group.GroupPassword == groupLogin.GroupPassword);
        }

        public async Task<List<Group>> FindGroupsInUser(string userName)
        {
            User currentUser = await _databaseContext.Users
                .Include(user => user.Groups)
                .FirstOrDefaultAsync(user => user.UserName == userName);
            return currentUser?.Groups.ToList();
        }

        public async Task<bool> CreateGroupAsync(GroupLogin groupLogin, User user)
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

        public async Task<bool> DeleteGroupAsync(GroupLogin groupLogin)
        {
            try
            {
                Group group = await FindGroupAsync(groupLogin);
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

        public async Task<bool> ChangeGroupLoginAsync(GroupLogin groupLogin, GroupLogin newGroupLogin)
        {
            try
            {
                Group group = await FindGroupAsync(groupLogin);
                if (group.GroupName != newGroupLogin.GroupName)
                {
                    if (await FindGroupAsync(groupLogin) != null) return false;
                }
                group.GroupName = newGroupLogin.GroupName;
                group.GroupPassword = newGroupLogin.GroupPassword;
                await _databaseContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            return true;
        }

        public async Task<bool> AddUserInGroupAsync(GroupLogin groupLogin, User user)
        {
            try
            {
                Group group = await FindGroupAsync(groupLogin);
                if (group == null) return false;
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

        public async Task<bool> RemoveUserFromGroupAsync(GroupLogin groupLogin, User user)
        {
            try
            {
                Group currentGroup = await _databaseContext.Groups
                    .Include(group => group.Users)
                    .FirstAsync(group => group.GroupName == groupLogin.GroupName &&
                                         group.GroupPassword == groupLogin.GroupPassword);
                if (currentGroup.Users.Count == 1)
                {
                    _databaseContext.Groups.Remove(currentGroup);
                }
                else
                {
                    currentGroup.Users.Remove(user);
                }
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
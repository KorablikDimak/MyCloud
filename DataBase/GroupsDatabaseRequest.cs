using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InfoLog;
using Microsoft.EntityFrameworkCore;
using MyCloud.DataBase.Interfaces;
using MyCloud.Models.Login;
using MyCloud.Models.User;

namespace MyCloud.DataBase
{
    public class GroupsDatabaseRequest : IGroupsRepository, IHaveLogger
    {
        private DataContext DatabaseContext { get; }
        public ILogger Logger { get; set; }

        public GroupsDatabaseRequest(DataContext context)
        {
            DatabaseContext = context;
        }

        public async Task<Group> FindGroupAsync(GroupLogin groupLogin)
        {
            return await DatabaseContext.Groups.FirstOrDefaultAsync(group => 
                group.Name == groupLogin.Name &&
                group.GroupPassword == groupLogin.GroupPassword);
        }

        public async Task<Group> FindGroupAsync(string groupName)
        {
            return await DatabaseContext.Groups.FirstOrDefaultAsync(group => 
                group.Name == groupName);
        }

        public async Task<List<Group>> FindGroupsInUser(string userName)
        {
            User currentUser = await DatabaseContext.Users
                .Include(user => user.Groups)
                .FirstOrDefaultAsync(user => user.UserName == userName);
            return currentUser?.Groups.ToList();
        }
        
        public async Task<List<User>> FindUsersInGroup(string groupName)
        {
            Group currentGroup = await DatabaseContext.Groups
                .Include(group => group.Users)
                .FirstOrDefaultAsync(group => group.Name == groupName);
            return currentGroup?.Users.ToList();
        }

        public async Task<bool> CreateGroupAsync(GroupLogin groupLogin, User user)
        {
            try
            {
                if (await FindGroupAsync(groupLogin) != null) return false;
                    
                var group = new Group
                {
                    Name = groupLogin.Name,
                    GroupPassword = groupLogin.GroupPassword
                };
                group.Users.Add(user);
                user.Groups.Add(group);
                await DatabaseContext.Groups.AddAsync(group);
                await DatabaseContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                await Logger?.Error(e.ToString())!;
                return false;
            }

            return true;
        }

        public async Task<bool> DeleteGroupAsync(GroupLogin groupLogin)
        {
            try
            {
                Group group = await FindGroupAsync(groupLogin);
                DatabaseContext.Groups.Remove(group);
                await DatabaseContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                await Logger?.Error(e.ToString())!;
                return false;
            }

            return true;
        }

        public async Task<bool> ChangeGroupLoginAsync(GroupLogin groupLogin, GroupLogin newGroupLogin)
        {
            try
            {
                Group group = await FindGroupAsync(groupLogin);
                if (group.Name != newGroupLogin.Name)
                {
                    if (await FindGroupAsync(newGroupLogin.Name) != null) return false;
                }
                group.Name = newGroupLogin.Name;
                group.GroupPassword = newGroupLogin.GroupPassword;
                await DatabaseContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                await Logger?.Error(e.ToString())!;
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

                if ((await FindUsersInGroup(group.Name))
                    .Any(targetUser => targetUser.UserName == user.UserName))
                    return false;

                group.Users.Add(user);
                user.Groups.Add(group);
                await DatabaseContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                await Logger?.Error(e.ToString())!;
                return false;
            }

            return true;
        }

        public async Task<bool> RemoveUserFromGroupAsync(GroupLogin groupLogin, User user)
        {
            try
            {
                Group currentGroup = await DatabaseContext.Groups
                    .Include(group => group.Users)
                    .FirstAsync(group => group.Name == groupLogin.Name &&
                                         group.GroupPassword == groupLogin.GroupPassword);
                if (currentGroup.Users.Count == 1)
                {
                    DatabaseContext.Groups.Remove(currentGroup);
                }
                else
                {
                    currentGroup.Users.Remove(user);
                }
                await DatabaseContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                await Logger?.Error(e.ToString())!;
                return false;
            }

            return true;
        }
    }
}
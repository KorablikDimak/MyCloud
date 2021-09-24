using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyCloud.DataBase.Interfaces;
using MyCloud.Models.User;

namespace MyCloud.DataBase
{
    public class DatabaseUsersRequest : IDatabaseUsersRequest
    {
        public DataContext DatabaseContext { private get; init; }

        public async Task<List<User>> FindUsersInGroup(GroupLogin groupLogin)
        {
            Group currentGroup = await DatabaseContext.Groups
                .Include(group => group.Users)
                .FirstOrDefaultAsync(group => group.GroupName == groupLogin.GroupName && 
                                              group.GroupPassword == groupLogin.GroupPassword);
            return currentGroup?.Users.ToList();
        }
        
        public async Task<User> FindUserAsync(string userName)
        {
            User user = await DatabaseContext.Users.FirstOrDefaultAsync(userData =>
                userData.UserName == userName);
            return user;
        }
        
        public async Task<User> FindUserAsync(string userName, string password)
        {
            User user = await DatabaseContext.Users.FirstOrDefaultAsync(userData =>
                userData.UserName == userName && 
                userData.Password == password);
            return user;
        }

        public async Task<bool> AddUserAsync(string userName, string password)
        {
            try
            {
                User user = await FindUserAsync(userName, password);
                if (user != null) return false;
                user = new User
                {
                    UserName = userName,
                    Password = password
                };
                await DatabaseContext.Users.AddAsync(user);
                await DatabaseContext.SaveChangesAsync();
                var personality = new PersonalityData
                {
                    Id = user.Id,
                    UserName = user.UserName
                };
                await DatabaseContext.Personality.AddAsync(personality);
                await DatabaseContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
            
            return true;
        }

        public async Task<bool> DeleteUserAsync(string userName, string password)
        {
            try
            {
                User user = await FindUserAsync(userName, password);
                if (user == null) return false;
                DatabaseContext.Users.Remove(user);
                await DatabaseContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            return true;
        }
        
        public async Task<bool> ChangePasswordAsync(string userName, string oldPassword, string newPassword)
        {
            try
            {
                User user = await FindUserAsync(userName, oldPassword);
                if (user == null) return false;
                user.Password = newPassword;
                await DatabaseContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
            
            return true;
        }
        
        public async Task<bool> ChangeUserNameAsync(Personality personality, string newUserName)
        {
            try
            {
                if (await FindUserAsync(newUserName) != null) return false;
                
                User user = await FindUserAsync(personality.UserName);
                if (user == null) return false;
                
                user.UserName = newUserName;
                personality.UserName = newUserName;
                await DatabaseContext.SaveChangesAsync();
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
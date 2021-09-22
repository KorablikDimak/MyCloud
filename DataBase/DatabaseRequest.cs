using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyCloud.DataBase.Interfaces;
using MyCloud.Models.MyFile;
using MyCloud.Models.User;

namespace MyCloud.DataBase
{
    public class DatabaseRequest : IDatabaseFilesRequest, IDatabasePersonalityRequest, IDatabaseUsersRequest
    {
        private readonly DataContext _databaseContext;

        public DatabaseRequest(DataContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public async Task<User> FindUserAsync(string userName, string password)
        {
            User user = await _databaseContext.Users.FirstOrDefaultAsync(userData =>
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
                await _databaseContext.Users.AddAsync(user);
                await _databaseContext.SaveChangesAsync();
                var personality = new PersonalityData
                {
                    Id = user.Id,
                    UserName = user.UserName
                };
                await _databaseContext.Personality.AddAsync(personality);
                await _databaseContext.SaveChangesAsync();
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
                _databaseContext.Users.Remove(user);
                await _databaseContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            return true;
        }

        public async Task<Personality> FindPersonalityAsync(string userName)
        {
            Personality personality = await _databaseContext.Personality.FirstOrDefaultAsync(personalityData => 
                personalityData.User.UserName == userName);
            return personality;
        }

        public async Task<bool> ChangeUserNameAsync(string oldUserName, string newUserName)
        {
            try
            {
                if (await FindUserAsync(newUserName) != null) return false;
                
                User user = await FindUserAsync(oldUserName);
                Personality personality = await FindPersonalityAsync(oldUserName);
                
                if (user == null || personality == null) return false;
                user.UserName = newUserName;
                personality.UserName = newUserName;
                await _databaseContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            return true;
        }

        public async Task<bool> ChangePersonalityAsync(string userName, Personality newPersonality)
        {
            try
            {
                Personality personality = await FindPersonalityAsync(userName);
                if (personality == null) return false;
                personality.Name = newPersonality.Name;
                personality.Surname = newPersonality.Surname;
                await _databaseContext.SaveChangesAsync();
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
                await _databaseContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
            
            return true;
        }

        public async Task<bool> AddFileAsync(string userName, MyFileInfo fileInfo)
        {
            try
            {
                User user = await FindUserAsync(userName);
                if (user == null) return false;
                fileInfo.User = user;
                await _databaseContext.Files.AddAsync(fileInfo);
                await _databaseContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            return true;
        }

        public async Task<User> FindUserAsync(string userName)
        {
            User user = await _databaseContext.Users.FirstOrDefaultAsync(userData =>
                userData.UserName == userName);
            return user;
        }

        public IQueryable<MyFileInfo> FindFiles(string userName)
        {
            IQueryable<MyFileInfo> files = _databaseContext.Files.Where(file => file.User.UserName == userName);
            return files;
        }

        public async Task<bool> DeleteFileAsync(string userName, string fileName)
        {
            try
            {
                MyFileInfo fileInfo = await _databaseContext.Files.FirstOrDefaultAsync(file => 
                    file.Name == fileName &&
                    file.User.UserName == userName);
                if (fileInfo == null) return false;
                _databaseContext.Files.Remove(fileInfo);
                await _databaseContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            return true;
        }

        public async Task<bool> DeleteAllFilesAsync(string userName)
        {
            try
            {
                _databaseContext.Files.RemoveRange(FindFiles(userName));
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
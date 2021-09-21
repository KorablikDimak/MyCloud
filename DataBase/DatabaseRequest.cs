using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyCloud.Models.MyFile;
using MyCloud.Models.User;

namespace MyCloud.DataBase
{
    public class DatabaseRequest : IDatabaseRequest
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
                await _databaseContext.Users.AddAsync(new User 
                { 
                    UserName = userName, 
                    Password = password
                });
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
        
        private async Task<User> FindUserAsync(string userName)
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
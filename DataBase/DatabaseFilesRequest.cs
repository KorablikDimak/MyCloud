using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyCloud.DataBase.Interfaces;
using MyCloud.Models.MyFile;
using MyCloud.Models.User;

namespace MyCloud.DataBase
{
    public class DatabaseFilesRequest : IDatabaseFilesRequest
    {
        private readonly DataContext _databaseContext;

        public DatabaseFilesRequest(DataContext databaseContext)
        {
            _databaseContext = databaseContext;
        }
        
        public async Task<bool> AddFileAsync(User user, MyFileInfo fileInfo)
        {
            try
            {
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
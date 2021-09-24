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
        public DataContext DatabaseContext { private get; init; }

        public async Task<bool> AddFileAsync(User user, MyFileInfo fileInfo)
        {
            try
            {
                fileInfo.User = user;
                await DatabaseContext.Files.AddAsync(fileInfo);
                await DatabaseContext.SaveChangesAsync();
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
            IQueryable<MyFileInfo> files = DatabaseContext.Files.Where(file => file.User.UserName == userName);
            return files;
        }
        
        public async Task<bool> DeleteFileAsync(string userName, string fileName)
        {
            try
            {
                MyFileInfo fileInfo = await DatabaseContext.Files.FirstOrDefaultAsync(file => 
                    file.Name == fileName &&
                    file.User.UserName == userName);
                if (fileInfo == null) return false;
                DatabaseContext.Files.Remove(fileInfo);
                await DatabaseContext.SaveChangesAsync();
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
                DatabaseContext.Files.RemoveRange(FindFiles(userName));
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
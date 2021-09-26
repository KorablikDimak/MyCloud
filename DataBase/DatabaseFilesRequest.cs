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
        public Group Group { private get; set; }
        public User User { private get; set; }
        
        private readonly DataContext _databaseContext;

        public DatabaseFilesRequest(DataContext context)
        {
            _databaseContext = context;
        }

        public async Task<bool> AddFileAsync(MyFileInfo fileInfo)
        {
            try
            {
                fileInfo.User = User;
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
        
        public IQueryable<MyFileInfo> FindFiles()
        {
            IQueryable<MyFileInfo> files = _databaseContext.Files.Where(file => file.User == User);
            return files;
        }
        
        public async Task<bool> DeleteFileAsync(string fileName)
        {
            try
            {
                MyFileInfo fileInfo = await _databaseContext.Files.FirstOrDefaultAsync(file => 
                    file.Name == fileName &&
                    file.User == User);
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
        
        public async Task<bool> DeleteAllFilesAsync()
        {
            try
            {
                _databaseContext.Files.RemoveRange(FindFiles());
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
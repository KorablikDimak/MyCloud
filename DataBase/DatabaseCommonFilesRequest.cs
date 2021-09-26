using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyCloud.DataBase.Interfaces;
using MyCloud.Models.MyFile;
using MyCloud.Models.User;

namespace MyCloud.DataBase
{
    public class DatabaseCommonFilesRequest : IDatabaseFilesRequest
    {
        private readonly DataContext _databaseContext;
        public Group Group { get; set; }
        public User User { get; set; }

        public DatabaseCommonFilesRequest(DataContext context)
        {
            _databaseContext = context;
        }

        public IQueryable<MyFileInfo> FindFiles()
        {
            IQueryable<MyFileInfo> files = _databaseContext.Files.Where(file => file.Group == Group);
            return files;
        }

        public async Task<bool> AddFileAsync(MyFileInfo fileInfo)
        {
            try
            {
                fileInfo.Group = Group;
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

        public async Task<bool> DeleteFileAsync(string fileName)
        {
            try
            {
                MyFileInfo fileInfo = await _databaseContext.Files.FirstOrDefaultAsync(file => 
                    file.Name == fileName &&
                    file.Group == Group);
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
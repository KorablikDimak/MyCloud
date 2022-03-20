using System;
using System.Linq;
using System.Threading.Tasks;
using InfoLog;
using Microsoft.EntityFrameworkCore;
using MyCloud.DataBase.Interfaces;
using MyCloud.Models.MyFile;
using MyCloud.Models.User;

namespace MyCloud.DataBase
{
    public class DatabaseFilesRequest : IDatabaseFilesRequest, IHaveLogger
    {
        private DataContext DatabaseContext { get; }
        public ILogger Logger { get; set; }

        public DatabaseFilesRequest(DataContext context)
        {
            DatabaseContext = context;
        }

        public async Task<bool> AddFileAsync<T>(MyFileInfo fileInfo, T criterion)
        {
            try
            {
                fileInfo.User = criterion as User;
                await DatabaseContext.Files.AddAsync(fileInfo);
                await DatabaseContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                await Logger?.Error(e.ToString())!;
                return false;
            }

            return true;
        }
        
        public IQueryable<MyFileInfo> FindFiles<T>(T criterion)
        {
            IQueryable<MyFileInfo> files = DatabaseContext.Files.Where(file => file.User == criterion as User);
            return files;
        }
        
        public async Task<bool> DeleteFileAsync<T>(string fileName, T criterion)
        {
            try
            {
                MyFileInfo fileInfo = await DatabaseContext.Files.FirstOrDefaultAsync(file => 
                    file.Name == fileName &&
                    file.User == criterion as User);
                if (fileInfo == null) return false;
                DatabaseContext.Files.Remove(fileInfo);
                await DatabaseContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                await Logger?.Error(e.ToString())!;
                return false;
            }

            return true;
        }
        
        public async Task<bool> DeleteAllFilesAsync<T>(T criterion)
        {
            try
            {
                DatabaseContext.Files.RemoveRange(FindFiles(criterion));
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
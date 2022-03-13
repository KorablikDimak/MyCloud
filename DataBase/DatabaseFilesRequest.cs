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
        private readonly DataContext _databaseContext;
        public ILogger Logger { get; set; }

        public DatabaseFilesRequest(DataContext context)
        {
            _databaseContext = context;
        }

        public async Task<bool> AddFileAsync<T>(MyFileInfo fileInfo, T criterion)
        {
            try
            {
                fileInfo.User = criterion as User;
                await _databaseContext.Files.AddAsync(fileInfo);
                await _databaseContext.SaveChangesAsync();
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
            IQueryable<MyFileInfo> files = _databaseContext.Files.Where(file => file.User == criterion as User);
            return files;
        }
        
        public async Task<bool> DeleteFileAsync<T>(string fileName, T criterion)
        {
            try
            {
                MyFileInfo fileInfo = await _databaseContext.Files.FirstOrDefaultAsync(file => 
                    file.Name == fileName &&
                    file.User == criterion as User);
                if (fileInfo == null) return false;
                _databaseContext.Files.Remove(fileInfo);
                await _databaseContext.SaveChangesAsync();
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
                _databaseContext.Files.RemoveRange(FindFiles(criterion));
                await _databaseContext.SaveChangesAsync();
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
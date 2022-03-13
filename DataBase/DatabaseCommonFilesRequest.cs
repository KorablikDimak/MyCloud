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
    public class DatabaseCommonFilesRequest : IDatabaseFilesRequest, IHaveLogger
    {
        private readonly DataContext _databaseContext;
        public ILogger Logger { get; set; }

        public DatabaseCommonFilesRequest(DataContext context)
        {
            _databaseContext = context;
        }

        public IQueryable<MyFileInfo> FindFiles<T>(T criterion)
        {
            IQueryable<MyFileInfo> files = _databaseContext.Files.Where(file => file.Group == criterion as Group);
            return files;
        }

        public async Task<bool> AddFileAsync<T>(MyFileInfo fileInfo, T criterion)
        {
            try
            {
                fileInfo.Group = criterion as Group;
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

        public async Task<bool> DeleteFileAsync<T>(string fileName, T criterion)
        {
            try
            {
                MyFileInfo fileInfo = await _databaseContext.Files.FirstOrDefaultAsync(file => 
                    file.Name == fileName &&
                    file.Group == criterion as Group);
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
                _databaseContext.Files.RemoveRange(FindFiles(criterion as Group));
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
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
    public class CommonFilesDatabaseRequest : IFilesRepository, IHaveLogger
    {
        private DataContext DatabaseContext { get; }
        public ILogger Logger { get; set; }

        public CommonFilesDatabaseRequest(DataContext context)
        {
            DatabaseContext = context;
        }

        public IQueryable<MyFileInfo> FindFiles<T>(T criterion)
        {
            IQueryable<MyFileInfo> files = DatabaseContext.Files.Where(file => file.Group == criterion as Group);
            return files;
        }

        public async Task<bool> AddFileAsync<T>(MyFileInfo fileInfo, T criterion)
        {
            try
            {
                fileInfo.Group = criterion as Group;
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

        public async Task<bool> DeleteFileAsync<T>(string fileName, T criterion)
        {
            try
            {
                MyFileInfo fileInfo = await DatabaseContext.Files.FirstOrDefaultAsync(file => 
                    file.Name == fileName &&
                    file.Group == criterion as Group);
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
                DatabaseContext.Files.RemoveRange(FindFiles(criterion as Group));
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
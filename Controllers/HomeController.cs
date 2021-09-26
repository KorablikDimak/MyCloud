using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyCloud.DataBase.DataRequestBuilder;
using MyCloud.DataBase.Interfaces;
using MyCloud.Models.MyFile;
using MyCloud.Models.User;

namespace MyCloud.Controllers
{
    public class HomeController : Controller
    {
        private const long MaxMemorySize = 10737418240;
        private readonly IDatabaseRequest _databaseRequest;

        public HomeController(HomeDataRequestBuilder homeDataRequestBuilder)
        {
            _databaseRequest = homeDataRequestBuilder.DatabaseRequest;
        }
        
        [Authorize]
        [HttpGet]
        public IActionResult MyFiles()
        {
            return View();
        }

        [Authorize]
        [RequestSizeLimit(1073741824)]
        [HttpPost("LoadFiles")]
        public async Task<IActionResult> LoadFiles(ICollection<IFormFile> files)
        {
            foreach (var file in files) 
            {
                string untrustedFileName = Path.GetFileName(file.FileName);
                string filePath = $"UserFiles\\{User.Identity.Name}\\{untrustedFileName}";
                if (!IsMemoryFree($"UserFiles\\{User.Identity.Name}", file.Length)) return new ConflictResult();
                if (await SaveFile(_databaseRequest.DatabaseFilesRequest, file, filePath)) continue;
                return new ConflictResult();
            }

            return Ok();
        }

        private async Task<bool> SaveFile(
            IDatabaseFilesRequest databaseFilesRequest, IFormFile file, string filePath)
        {
            if (System.IO.File.Exists(filePath)) return false;
                
            await using var stream = System.IO.File.Create(filePath);
            var fileInfo = new FileInfo(stream.Name);
            var myFileInfo = new MyFileInfo 
            {
                Name = fileInfo.Name, 
                TypeOfFile = fileInfo.Extension, 
                DateTime = fileInfo.CreationTime,
                Size = file.Length
            };

            if (!await SetUser(databaseFilesRequest)) return false;
            bool isAdded = await databaseFilesRequest.AddFileAsync(myFileInfo);
            if (isAdded)
            {
                await file.CopyToAsync(stream);
            }
            else
            {
                stream.Close();
                System.IO.File.Delete(filePath);
                return false;
            }

            return true;
        }

        private async Task<bool> SetUser(IDatabaseFilesRequest databaseFilesRequest)
        {
            User user = await _databaseRequest.DatabaseUsersRequest.FindUserAsync(User.Identity.Name);
            if (user == null) return false;
            databaseFilesRequest.User = user;
            return true;
        }

        private bool IsMemoryFree(string path, long fileSize)
        {
            var dirInfo = new DirectoryInfo(path);
            return dirInfo.GetFiles().Sum(file => file.Length) + fileSize <= MaxMemorySize;
        }

        [Authorize]
        [HttpPost("GetFileInfo")]
        public async Task<List<MyFileInfo>> GetFileInfo([FromBody] SortType sortType)
        {
            return await GetFileInfos(_databaseRequest.DatabaseFilesRequest, sortType);
        }

        private async Task<List<MyFileInfo>> GetFileInfos(IDatabaseFilesRequest databaseFilesRequest, SortType sortType)
        {
            if (!await SetUser(databaseFilesRequest)) return new List<MyFileInfo>();
            IQueryable<MyFileInfo> files = databaseFilesRequest.FindFiles();

            files = sortType.OrderBy switch
            {
                "typeoffile" => files.OrderBy(file => file.TypeOfFile),
                "datetime" => files.OrderBy(file => file.DateTime),
                "size" => files.OrderBy(file => file.Size),
                _ => files.OrderBy(file => file.Name)
            };
            if (sortType.TypeOfSort == "DESC")
            {
                files = files.Reverse();
            }

            return files.Select(file => new MyFileInfo
            {
                Name = file.Name,
                TypeOfFile = file.TypeOfFile
            }).ToList();
        }

        [Authorize]
        [RequestSizeLimit(1073741824)]
        [HttpPost("GetFile")]
        public VirtualFileResult GetVirtualFile([FromBody] string fileName)
        {
            return GetFile(fileName, $"~/UserFiles/{User.Identity.Name}");
        }

        private VirtualFileResult GetFile(string fileName, string path)
        {
            string filepath = Path.Combine(path, fileName);
            return File(filepath, "application/octet-stream", fileName);
        }

        [Authorize]
        [HttpDelete("DeleteOneFile")]
        public async Task<IActionResult> DeleteOneFile([FromBody] string fileName)
        {
            string filepath = Path.Combine($"UserFiles\\{User.Identity.Name}", fileName);
            bool isDeleted = await DeleteFile(_databaseRequest.DatabaseFilesRequest, fileName, filepath);
            if (isDeleted) return Ok();
            return new ConflictResult();
        }

        private async Task<bool> DeleteFile(
            IDatabaseFilesRequest databaseFilesRequest, string fileName, string filepath)
        {
            if (!await SetUser(databaseFilesRequest)) return false;
            bool isDeleted = await databaseFilesRequest.DeleteFileAsync(fileName);
            if (!isDeleted) return false;
            System.IO.File.Delete(filepath);
            return true;
        }

        [Authorize]
        [HttpDelete("DeleteAllFiles")]
        public async Task<IActionResult> DeleteAllFiles()
        {
            bool isDeleted = await DeleteAll(_databaseRequest.DatabaseFilesRequest, $"UserFiles\\{User.Identity.Name}");
            if (isDeleted) return Ok();
            return new ConflictResult();
        }

        private async Task<bool> DeleteAll(IDatabaseFilesRequest databaseFilesRequest, string filePath)
        {
            if (!await SetUser(databaseFilesRequest)) return false;
            bool isDeleted = await databaseFilesRequest.DeleteAllFilesAsync();
            if (!isDeleted) return false;
            
            var dirInfo = new DirectoryInfo(filePath);
            foreach (var file in dirInfo.GetFiles())
            {
                file.Delete();
            }

            return true;
        }

        [Authorize]
        [HttpGet("GetMemorySize")]
        public long GetMemorySize()
        {
            return GetDirectorySize($"UserFiles\\{User.Identity.Name}");
        }

        private long GetDirectorySize(string path)
        {
            var dirInfo = new DirectoryInfo(path);
            return dirInfo.GetFiles().Sum(file => file.Length);
        }

        [Authorize]
        [HttpGet("Logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok();
        }
    }
}
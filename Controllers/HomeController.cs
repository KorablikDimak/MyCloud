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
    [Authorize]
    public class HomeController : Controller
    {
        private const long MaxMemorySize = 10737418240;
        private const long RequestSizeLimit = 1073741824;
        private readonly IDatabaseRequest _databaseRequest;

        public HomeController(HomeDataRequestBuilder homeDataRequestBuilder)
        {
            _databaseRequest = homeDataRequestBuilder.DatabaseRequest;
        }
        
        [HttpGet]
        public IActionResult MyFiles()
        {
            return View();
        }
        
        [RequestSizeLimit(RequestSizeLimit)]
        [HttpPost("LoadFiles")]
        public async Task<IActionResult> LoadFiles(ICollection<IFormFile> files)
        {
            long size = 0;
            foreach (var file in files)
            {
                size += file.Length;
            }
            if (!(GetDirectorySize($"UserFiles\\{User.Identity.Name}") + size <= MaxMemorySize))
                return new ConflictResult();
            
            if (!await SetUser(_databaseRequest.DatabaseFilesRequest)) return new ConflictResult();
            bool isSaved =
                await SaveFiles(_databaseRequest.DatabaseFilesRequest, files, $"UserFiles\\{User.Identity.Name}");
            if (isSaved) return Ok();
            return new ConflictResult();
        }

        private async Task<bool> SaveFiles(
            IDatabaseFilesRequest databaseFilesRequest, ICollection<IFormFile> files, string path)
        {
            foreach (var file in files)
            {
                string untrustedFileName = Path.GetFileName(file.FileName);
                string filePath = path + $"\\{untrustedFileName}";
                
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
        
        [HttpPost("GetFileInfo")]
        public async Task<List<MyFileInfo>> GetFileInfo([FromBody] SortType sortType)
        {
            if (!await SetUser(_databaseRequest.DatabaseFilesRequest)) return new List<MyFileInfo>();
            return GetFileInfos(_databaseRequest.DatabaseFilesRequest, sortType);
        }

        private List<MyFileInfo> GetFileInfos(IDatabaseFilesRequest databaseFilesRequest, SortType sortType)
        {
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
        
        [RequestSizeLimit(RequestSizeLimit)]
        [HttpPost("GetVirtualFile")]
        public IActionResult GetVirtualFile([FromBody] string fileName)
        {
            return GetFile(fileName, $"UserFiles\\{User.Identity.Name}");
        }

        private IActionResult GetFile(string fileName, string path)
        {
            string filepath = Path.Combine(path, fileName);
            Stream fileStream = new FileStream(filepath, FileMode.Open);
            return File(fileStream, "application/octet-stream");
        }
        
        [HttpDelete("DeleteOneFile")]
        public async Task<IActionResult> DeleteOneFile([FromBody] string fileName)
        {
            if (!await SetUser(_databaseRequest.DatabaseFilesRequest)) return new ConflictResult();
            string filepath = Path.Combine($"UserFiles\\{User.Identity.Name}", fileName);
            bool isDeleted = await DeleteFile(_databaseRequest.DatabaseFilesRequest, fileName, filepath);
            if (isDeleted) return Ok();
            return new ConflictResult();
        }

        private async Task<bool> DeleteFile(
            IDatabaseFilesRequest databaseFilesRequest, string fileName, string filepath)
        {
            bool isDeleted = await databaseFilesRequest.DeleteFileAsync(fileName);
            if (!isDeleted) return false;
            System.IO.File.Delete(filepath);
            return true;
        }
        
        [HttpDelete("DeleteAllFiles")]
        public async Task<IActionResult> DeleteAllFiles()
        {
            if (!await SetUser(_databaseRequest.DatabaseFilesRequest)) return new ConflictResult();
            bool isDeleted = await DeleteAll(_databaseRequest.DatabaseFilesRequest, 
                $"UserFiles\\{User.Identity.Name}");
            if (isDeleted) return Ok();
            return new ConflictResult();
        }

        private async Task<bool> DeleteAll(IDatabaseFilesRequest databaseFilesRequest, string filePath)
        {
            bool isDeleted = await databaseFilesRequest.DeleteAllFilesAsync();
            if (!isDeleted) return false;
            
            var dirInfo = new DirectoryInfo(filePath);
            foreach (var file in dirInfo.GetFiles())
            {
                file.Delete();
            }

            return true;
        }
        
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
        
        [HttpGet("Logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok();
        }
        
        [HttpPost("GetCommonFileInfo")]
        public async Task<List<MyFileInfo>> GetCommonFileInfo([FromBody] SortType sortType,
            [FromHeader(Name = "GroupName")] string groupName, [FromHeader(Name = "GroupPassword")] string groupPassword)
        {
            GroupLogin groupLogin = new GroupLogin
            {
                Name = groupName,
                GroupPassword = groupPassword
            };
            if (!await SetGroup(_databaseRequest.DatabaseCommonFilesRequest, groupLogin)) return new List<MyFileInfo>();
            return GetFileInfos(_databaseRequest.DatabaseCommonFilesRequest, sortType);
        }

        private async Task<bool> SetGroup(IDatabaseFilesRequest databaseFilesRequest, GroupLogin groupLogin)
        {
            Group group = await _databaseRequest.DatabaseGroupsRequest.FindGroupAsync(groupLogin);
            if (group == null) return false;
            databaseFilesRequest.Group = group;
            return true;
        }
        
        [RequestSizeLimit(RequestSizeLimit)]
        [HttpPost("GetCommonVirtualFile")]
        public async Task<IActionResult> GetCommonVirtualFile([FromBody] string fileName, 
            [FromHeader(Name = "GroupName")] string groupName, [FromHeader(Name = "GroupPassword")] string groupPassword)
        {
            GroupLogin groupLogin = new GroupLogin
            {
                Name = groupName,
                GroupPassword = groupPassword
            };
            Group group = await _databaseRequest.DatabaseGroupsRequest.FindGroupAsync(groupLogin);
            if (group == null) return null;
            return GetFile(fileName, $"CommonFiles\\{group.Name}");
        }
        
        [HttpPost("LoadCommonFiles")]
        public async Task<IActionResult> LoadCommonFiles(ICollection<IFormFile> files, 
            [FromHeader(Name = "GroupName")] string groupName, [FromHeader(Name = "GroupPassword")] string groupPassword)
        {
            GroupLogin groupLogin = new GroupLogin
            {
                Name = groupName,
                GroupPassword = groupPassword
            };
            long size = 0;
            foreach (var file in files)
            {
                size += file.Length;
            }
            if (!(await CountCommonMemorySize() + size <= MaxMemorySize)) return new ConflictResult();
            
            if (!await SetGroup(_databaseRequest.DatabaseCommonFilesRequest, groupLogin)) return new ConflictResult();
            bool isSaved = await SaveFiles(_databaseRequest.DatabaseCommonFilesRequest, files,
                $"CommonFiles\\{groupLogin.Name}");
            if (isSaved) return Ok();
            return new ConflictResult();
        }
        
        [HttpDelete("DeleteOneCommonFile")]
        public async Task<IActionResult> DeleteOneCommonFile([FromBody] string fileName,
            [FromHeader(Name = "GroupName")] string groupName, [FromHeader(Name = "GroupPassword")] string groupPassword)
        {
            GroupLogin groupLogin = new GroupLogin
            {
                Name = groupName,
                GroupPassword = groupPassword
            };
            if (!await SetGroup(_databaseRequest.DatabaseCommonFilesRequest, groupLogin)) return new ConflictResult();
            string filePath = $"CommonFiles\\{groupLogin.Name}";
            bool isDeleted = await DeleteFile(_databaseRequest.DatabaseCommonFilesRequest, fileName, filePath);
            if (isDeleted) return Ok();
            return new ConflictResult();
        }
        
        [HttpDelete("DeleteAllCommonFiles")]
        public async Task<IActionResult> DeleteAllCommonFiles(
            [FromHeader(Name = "GroupName")] string groupName, [FromHeader(Name = "GroupPassword")] string groupPassword)
        {
            GroupLogin groupLogin = new GroupLogin
            {
                Name = groupName,
                GroupPassword = groupPassword
            };
            if (!await SetGroup(_databaseRequest.DatabaseCommonFilesRequest, groupLogin)) return new ConflictResult();
            string filePath = $"CommonFiles\\{groupLogin.Name}";
            bool isDeleted = await DeleteAll(_databaseRequest.DatabaseCommonFilesRequest, filePath);
            if (isDeleted) return Ok();
            return new ConflictResult();
        }
        
        [HttpGet("GetCommonMemorySize")]
        public async Task<long> GetCommonMemorySize()
        {
            return await CountCommonMemorySize();
        }

        private async Task<long> CountCommonMemorySize()
        {
            List<Group> groups = await _databaseRequest.DatabaseGroupsRequest.FindGroupsInUser(User.Identity.Name);
            long commonMemorySize = 0;
            foreach (var group in groups)
            {
                commonMemorySize += GetDirectorySize($"CommonFiles\\{group.Name}");
            }

            return commonMemorySize;
        }
    }
}
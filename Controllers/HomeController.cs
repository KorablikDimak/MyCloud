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
        private readonly DatabaseRequest _databaseRequest;

        public HomeController(DatabaseRequest databaseRequest)
        {
            _databaseRequest = databaseRequest;
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

            User user = await _databaseRequest.DatabaseUsersRequest.FindUserAsync(User.Identity.Name);
            if (user == null)
            {
                return new ConflictResult();
            }

            bool isSaved =
                await SaveFilesAsync(_databaseRequest.DatabaseFilesRequest, files, $"UserFiles\\{User.Identity.Name}", user);
            if (isSaved) return Ok();
            return new ConflictResult();
        }

        private async Task<bool> SaveFilesAsync<T>(
            IDatabaseFilesRequest databaseFilesRequest, ICollection<IFormFile> files, string path, T criterion)
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

                bool isAdded = await databaseFilesRequest.AddFileAsync(myFileInfo, criterion);
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
        
        [HttpPost("GetFileInfo")]
        public async Task<List<MyFileInfo>> GetFileInfo([FromBody] SortType sortType)
        {
            User user = await _databaseRequest.DatabaseUsersRequest.FindUserAsync(User.Identity.Name);
            return user == null ? new List<MyFileInfo>() : GetFileInfos(_databaseRequest.DatabaseFilesRequest, sortType, user);
        }

        private List<MyFileInfo> GetFileInfos<T>(IDatabaseFilesRequest databaseFilesRequest, SortType sortType, T criterion)
        {
            IQueryable<MyFileInfo> files = databaseFilesRequest.FindFiles(criterion);

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
            string filepath = Path.Combine($"UserFiles\\{User.Identity.Name}", fileName);
            
            User user = await _databaseRequest.DatabaseUsersRequest.FindUserAsync(User.Identity.Name);
            if (user == null)
            {
                return new ConflictResult();
            }
            
            bool isDeleted = await DeleteFileAsync(_databaseRequest.DatabaseFilesRequest, fileName, filepath, user);
            if (isDeleted) return Ok();
            return new ConflictResult();
        }

        private async Task<bool> DeleteFileAsync<T>(
            IDatabaseFilesRequest databaseFilesRequest, string fileName, string filepath, T criterion)
        {
            bool isDeleted = await databaseFilesRequest.DeleteFileAsync(fileName, criterion);
            if (!isDeleted) return false;
            System.IO.File.Delete(filepath);
            return true;
        }
        
        [HttpDelete("DeleteAllFiles")]
        public async Task<IActionResult> DeleteAllFiles()
        {
            User user = await _databaseRequest.DatabaseUsersRequest.FindUserAsync(User.Identity.Name);
            if (user == null)
            {
                return new ConflictResult();
            }
            
            bool isDeleted = await DeleteAllAsync(_databaseRequest.DatabaseFilesRequest, 
                $"UserFiles\\{User.Identity.Name}", user);
            if (isDeleted) return Ok();
            return new ConflictResult();
        }

        private async Task<bool> DeleteAllAsync<T>(IDatabaseFilesRequest databaseFilesRequest, string filePath, T criterion)
        {
            bool isDeleted = await databaseFilesRequest.DeleteAllFilesAsync(criterion);
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

            Group group = await _databaseRequest.DatabaseGroupsRequest.FindGroupAsync(groupLogin);
            return group == null ? new List<MyFileInfo>() : 
                GetFileInfos(_databaseRequest.DatabaseCommonFilesRequest, sortType, group);
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
            return group == null ? null : 
                GetFile(fileName, $"CommonFiles\\{group.Name}");
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
            
            Group group = await _databaseRequest.DatabaseGroupsRequest.FindGroupAsync(groupLogin);
            if (group == null)
            {
                return new ConflictResult();
            }
            
            bool isSaved = await SaveFilesAsync(_databaseRequest.DatabaseCommonFilesRequest, files,
                $"CommonFiles\\{groupLogin.Name}", group);
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
            
            Group group = await _databaseRequest.DatabaseGroupsRequest.FindGroupAsync(groupLogin);
            if (group == null)
            {
                return new ConflictResult();
            }
            
            string filePath = Path.Combine($"CommonFiles\\{groupLogin.Name}", fileName);
            bool isDeleted = await DeleteFileAsync(_databaseRequest.DatabaseCommonFilesRequest, fileName, filePath, group);
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
            
            Group group = await _databaseRequest.DatabaseGroupsRequest.FindGroupAsync(groupLogin);
            if (group == null)
            {
                return new ConflictResult();
            }
            
            string filePath = $"CommonFiles\\{groupLogin.Name}";
            bool isDeleted = await DeleteAllAsync(_databaseRequest.DatabaseCommonFilesRequest, filePath, group);
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
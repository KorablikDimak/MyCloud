using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using InfoLog;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyCloud.DataBase.DataRequestBuilder;
using MyCloud.DataBase.Interfaces;
using MyCloud.Models.Login;
using MyCloud.Models.MyFile;
using MyCloud.Models.User;

namespace MyCloud.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private const long MaxMemorySize = 10737418240;
        private const long RequestSizeLimit = 1073741824;
        private Repository Repository { get; }
        private ILogger Logger { get; }

        public HomeController(Repository repository, ILogger logger)
        {
            repository.ImplementLogger(logger);
            Repository = repository;
            Logger = logger;
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

            User user = await Repository.UsersRepository.FindUserAsync(User.Identity.Name);
            if (user == null)
            {
                return new ConflictResult();
            }

            bool isSaved =
                await SaveFilesAsync(Repository.FilesRepository, files, $"UserFiles\\{User.Identity.Name}", user);
            if (isSaved) return Ok();
            return new ConflictResult();
        }

        private async Task<bool> SaveFilesAsync<T>(
            IFilesRepository filesRepository, ICollection<IFormFile> files, string path, T criterion)
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

                bool isAdded = await filesRepository.AddFileAsync(myFileInfo, criterion);
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
            User user = await Repository.UsersRepository.FindUserAsync(User.Identity.Name);
            return user == null ? new List<MyFileInfo>() : GetFileInfos(Repository.FilesRepository, sortType, user);
        }

        private List<MyFileInfo> GetFileInfos<T>(IFilesRepository filesRepository, SortType sortType, T criterion)
        {
            IQueryable<MyFileInfo> files = filesRepository.FindFiles(criterion);

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
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filepath = Path.Combine(path, fileName);
            Stream fileStream = new FileStream(filepath, FileMode.Open);
            return File(fileStream, "application/octet-stream");
        }
        
        [HttpDelete("DeleteOneFile")]
        public async Task<IActionResult> DeleteOneFile([FromBody] string fileName)
        {
            User user = await Repository.UsersRepository.FindUserAsync(User.Identity.Name);
            if (user == null)
            {
                return new ConflictResult();
            }
            
            string filepath = Path.Combine($"UserFiles\\{User.Identity.Name}", fileName);
            bool isDeleted = await DeleteFileAsync(Repository.FilesRepository, fileName, filepath, user);
            if (isDeleted) return Ok();
            return new ConflictResult();
        }

        private async Task<bool> DeleteFileAsync<T>(
            IFilesRepository filesRepository, string fileName, string filepath, T criterion)
        {
            bool isDeleted = await filesRepository.DeleteFileAsync(fileName, criterion);
            if (!isDeleted) return false;
            System.IO.File.Delete(filepath);
            return true;
        }
        
        [HttpDelete("DeleteAllFiles")]
        public async Task<IActionResult> DeleteAllFiles()
        {
            User user = await Repository.UsersRepository.FindUserAsync(User.Identity.Name);
            if (user == null)
            {
                return new ConflictResult();
            }
            
            bool isDeleted = await DeleteAllAsync(Repository.FilesRepository, 
                $"UserFiles\\{User.Identity.Name}", user);
            if (isDeleted) return Ok();
            return new ConflictResult();
        }

        private async Task<bool> DeleteAllAsync<T>(IFilesRepository filesRepository, string filePath, T criterion)
        {
            bool isDeleted = await filesRepository.DeleteAllFilesAsync(criterion);
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
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            var dirInfo = new DirectoryInfo(path);
            return dirInfo.GetFiles().Sum(file => file.Length);
        }
        
        [HttpGet("Logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("~/Account/Login");
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

            Group group = await Repository.GroupsRepository.FindGroupAsync(groupLogin);
            return group == null ? new List<MyFileInfo>() : 
                GetFileInfos(Repository.CommonFilesRepository, sortType, group);
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

            Group group = await Repository.GroupsRepository.FindGroupAsync(groupLogin);

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
            
            Group group = await Repository.GroupsRepository.FindGroupAsync(groupLogin);
            if (group == null)
            {
                return new ConflictResult();
            }
            
            bool isSaved = await SaveFilesAsync(Repository.CommonFilesRepository, files,
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
            
            Group group = await Repository.GroupsRepository.FindGroupAsync(groupLogin);
            if (group == null)
            {
                return new ConflictResult();
            }
            
            string filePath = Path.Combine($"CommonFiles\\{groupLogin.Name}", fileName);
            bool isDeleted = await DeleteFileAsync(Repository.CommonFilesRepository, fileName, filePath, group);
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

            Group group = await Repository.GroupsRepository.FindGroupAsync(groupLogin);
            if (group == null)
            {
                return new ConflictResult();
            }
            
            string filePath = $"CommonFiles\\{groupLogin.Name}";
            bool isDeleted = await DeleteAllAsync(Repository.CommonFilesRepository, filePath, group);
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
            List<Group> groups = await Repository.GroupsRepository.FindGroupsInUser(User.Identity.Name);

            return groups.Sum(group => GetDirectorySize($"CommonFiles\\{group.Name}"));
        }
    }
}
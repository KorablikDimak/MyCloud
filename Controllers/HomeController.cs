using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyCloud.DataBase.Interfaces;
using MyCloud.Models.MyFile;
using MyCloud.Models.User;

namespace MyCloud.Controllers
{
    public class HomeController : Controller
    {
        private const long MaxMemorySize = 10737418240;
        private readonly IDatabaseFilesRequest _databaseFilesRequest;
        private readonly IDatabaseUsersRequest _databaseUsersRequest;

        public HomeController(IDatabaseFilesRequest databaseFilesRequest, IDatabaseUsersRequest databaseUsersRequest)
        {
            _databaseFilesRequest = databaseFilesRequest;
            _databaseUsersRequest = databaseUsersRequest;
        }
        
        [Authorize]
        [HttpGet]
        public IActionResult MyFiles()
        {
            return View();
        }

        [Authorize]
        [RequestSizeLimit(1073741824)]
        [HttpPost("LoadFile")]
        public async Task<IActionResult> LoadFile(ICollection<IFormFile> files)
        {
            foreach (var file in files) 
            {
                if (User.Identity == null) return new ConflictResult();
                
                string untrustedFileName = Path.GetFileName(file.FileName);
                string filePath = $"UserFiles\\{User.Identity.Name}\\{untrustedFileName}";
                if (System.IO.File.Exists(filePath)) return new ConflictResult();
                
                await using var stream = System.IO.File.Create(filePath);
                if (!IsMemoryFree(file.Length)) return new ConflictResult();
                
                var fileInfo = new FileInfo(stream.Name);
                var myFileInfo = new MyFileInfo 
                    {
                        Name = fileInfo.Name, 
                        TypeOfFile = fileInfo.Extension, 
                        DateTime = fileInfo.CreationTime,
                        Size = file.Length
                    };

                User user = await _databaseUsersRequest.FindUserAsync(User.Identity.Name);
                if (user == null) return new ConflictResult();
                bool isAdded = await _databaseFilesRequest.AddFileAsync(user, myFileInfo);
                if (isAdded)
                {
                    await file.CopyToAsync(stream);
                }
                else
                {
                    stream.Close();
                    System.IO.File.Delete(filePath);
                    return new ConflictResult();
                }
            }

            return Ok();
        }

        private bool IsMemoryFree(long fileSize)
        {
            if (User.Identity == null) return false;
            var dirInfo = new DirectoryInfo($"UserFiles\\{User.Identity.Name}");
            return dirInfo.GetFiles().Sum(file => file.Length) + fileSize <= MaxMemorySize;
        }

        [Authorize]
        [HttpPost("GetFileInfo")]
        public List<MyFileInfo> GetFileInfo([FromBody] SortType sortType)
        {
            if (User.Identity == null) return null;
            
            IQueryable<MyFileInfo> files = _databaseFilesRequest.FindFiles(User.Identity.Name);
            
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
            
            return files.ToList();
        }

        [Authorize]
        [RequestSizeLimit(1073741824)]
        [HttpPost("GetFile")]
        public VirtualFileResult GetVirtualFile([FromBody] string fileName)
        {
            if (User.Identity == null) return null;
            string filepath = Path.Combine($"~/UserFiles/{User.Identity.Name}", fileName);
            return File(filepath, "application/octet-stream", fileName);
        }

        [Authorize]
        [HttpDelete("DeleteOneFile")]
        public async Task<IActionResult> DeleteOneFile([FromBody] string fileName)
        {
            if (User.Identity == null) return new ConflictResult();
            string filepath = Path.Combine($"UserFiles\\{User.Identity.Name}", fileName);
            bool isDeleted = await _databaseFilesRequest.DeleteFileAsync(User.Identity.Name, fileName);
            if (!isDeleted) return new ConflictResult();
            System.IO.File.Delete(filepath);
            return Ok();
        }

        [Authorize]
        [HttpDelete("DeleteAllFiles")]
        public async Task<IActionResult> DeleteAllFiles()
        {
            if (User.Identity == null) return new ConflictResult();
            
            bool isDeleted = await _databaseFilesRequest.DeleteAllFilesAsync(User.Identity.Name);
            if (!isDeleted) return new ConflictResult();
            
            var dirInfo = new DirectoryInfo($"UserFiles\\{User.Identity.Name}");
            foreach (var file in dirInfo.GetFiles())
            {
                file.Delete();
            }
            
            return Ok();
        }

        [Authorize]
        [HttpGet("GetMemorySize")]
        public long GetMemorySize()
        {
            if (User.Identity == null) return 0;
            var dirInfo = new DirectoryInfo($"UserFiles\\{User.Identity.Name}");
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
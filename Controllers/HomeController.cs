using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyCloud.DataBase;
using MyCloud.Models.MyFile;

namespace MyCloud.Controllers
{
    public class HomeController : Controller
    {
        private const long MaxMemorySize = 10737418240;
        private readonly IDatabaseRequest _databaseRequest;

        public HomeController(DataContext context)
        {
            _databaseRequest = new DatabaseRequest(context);
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
                string filePath = $"wwwroot\\data\\{User.Identity.Name}\\{untrustedFileName}";
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

                bool isAdded = await _databaseRequest.AddFileAsync(User.Identity.Name, myFileInfo);
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
            var dirInfo = new DirectoryInfo($"wwwroot\\data\\{User.Identity.Name}");
            return dirInfo.GetFiles().Sum(file => file.Length) + fileSize <= MaxMemorySize;
        }

        [Authorize]
        [HttpPost("GetFileInfo")]
        public List<MyFileInfo> GetFileInfo([FromBody] SortType sortType)
        {
            if (User.Identity == null) return null;
            
            IQueryable<MyFileInfo> files = _databaseRequest.FindFiles(User.Identity.Name);
            
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
            string filepath = Path.Combine($"~/data/{User.Identity.Name}", fileName);
            return File(filepath, "application/octet-stream", fileName);
        }

        [Authorize]
        [HttpDelete("DeleteOneFile")]
        public async Task<IActionResult> DeleteOneFile([FromBody] string fileName)
        {
            if (User.Identity == null) return new ConflictResult();
            string filepath = Path.Combine($"wwwroot\\data\\{User.Identity.Name}", fileName);
            bool isDeleted = await _databaseRequest.DeleteFileAsync(User.Identity.Name, fileName);
            if (!isDeleted) return new ConflictResult();
            System.IO.File.Delete(filepath);
            return Ok();
        }

        [Authorize]
        [HttpDelete("DeleteAllFiles")]
        public async Task<IActionResult> DeleteAllFiles()
        {
            if (User.Identity == null) return new ConflictResult();
            var dirInfo = new DirectoryInfo($"wwwroot\\data\\{User.Identity.Name}");
            foreach (var file in dirInfo.GetFiles())
            {
                bool isDeleted = await _databaseRequest.DeleteFileAsync(User.Identity.Name, file.Name);
                if (!isDeleted) return new ConflictResult();
                file.Delete();
            }
            
            return Ok();
        }

        [Authorize]
        [HttpGet("GetMemorySize")]
        public long GetMemorySize()
        {
            if (User.Identity == null) return 0;
            var dirInfo = new DirectoryInfo($"wwwroot\\data\\{User.Identity.Name}");
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
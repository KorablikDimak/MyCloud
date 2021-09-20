using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyCloud.Models.MyFile;

namespace MyCloud.Controllers
{
    public class HomeController : Controller
    {
        private const long MaxMemorySize = 10737418240;
        private readonly MyFileInfoContext _databaseContext;

        public HomeController(MyFileInfoContext context)
        {
            _databaseContext = context;
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
                var untrustedFileName = Path.GetFileName(file.FileName);
                var filePath = $"wwwroot\\data\\{untrustedFileName}";
                await using var stream = System.IO.File.Create(filePath);
                if (!IsMemoryFree(file.Length)) return new ConflictResult();
                
                var fileInfo = new FileInfo(filePath);
                var myFileInfo = new MyFileInfo 
                    {
                        Name = fileInfo.Name, 
                        TypeOfFile = fileInfo.Extension, 
                        Size = file.Length
                    };

                if (await LoadFileInfoToDataBaseAsync(myFileInfo))
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

        private async Task<bool> LoadFileInfoToDataBaseAsync(MyFileInfo fileInfo)
        {
            try
            {
                await _databaseContext.Files.AddAsync(fileInfo);
                await _databaseContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
            
            return true;
        }

        private bool IsMemoryFree(long fileSize)
        {
            var dirInfo = new DirectoryInfo("wwwroot\\data\\");
            return dirInfo.GetFiles().Sum(file => file.Length) + fileSize <= MaxMemorySize;
        }

        [Authorize]
        [HttpPost("GetFileInfo")]
        public List<MyFileInfo> GetFileInfo([FromBody] SortType sortType)
        {
            var fileInfoToSend = new List<MyFileInfo>(_databaseContext.Files.OrderBy(file => sortType.OrderBy));
            return fileInfoToSend;
        }

        [Authorize]
        [RequestSizeLimit(1073741824)]
        [HttpPost("GetFile")]
        public VirtualFileResult GetVirtualFile([FromBody] string fileName)
        {
            string filepath = Path.Combine("~/data", fileName);
            return File(filepath, "application/octet-stream", fileName);
        }

        [Authorize]
        [HttpDelete("DeleteOneFile")]
        public async Task<IActionResult> DeleteOneFile([FromBody] string fileName)
        {
            string filepath = Path.Combine("wwwroot\\data", fileName);
            if (!await DeleteFileFromDataBaseAsync(fileName)) return new ConflictResult();
            System.IO.File.Delete(filepath);
            return Ok();
        }

        [Authorize]
        [HttpDelete("DeleteAllFiles")]
        public async Task<IActionResult> DeleteAllFiles()
        {
            var dirInfo = new DirectoryInfo("wwwroot\\data\\");
            foreach (var file in dirInfo.GetFiles())
            {
                if (await DeleteFileFromDataBaseAsync(file.Name))
                {
                    file.Delete();
                }
                else
                {
                    return new ConflictResult();
                }
            }
            
            return Ok();
        }

        private async Task<bool> DeleteFileFromDataBaseAsync(string name)
        {
            try
            {
                MyFileInfo fileInfo = await _databaseContext.Files.FirstOrDefaultAsync(file => file.Name == name);
                if (fileInfo == null) return false;
                    
                _databaseContext.Files.Remove(fileInfo);
                await _databaseContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            return true;
        }

        [Authorize]
        [HttpGet("GetMemorySize")]
        public long GetMemorySize()
        {
            var dirInfo = new DirectoryInfo("wwwroot\\data\\");
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
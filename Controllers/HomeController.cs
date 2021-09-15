using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyCloud.Models.MyFile;

namespace MyCloud.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [RequestSizeLimit(1024 * 1024 * 1024)]
        [HttpPost]
        public async Task<IActionResult> Index(ICollection<IFormFile> files)
        {
            foreach (var file in files) 
            {
                string untrustedFileName = Path.GetFileName(file.FileName);
                await using var stream = System.IO.File.Create($"wwwroot\\data\\{untrustedFileName}");
                await file.CopyToAsync(stream);
            }

            return View();
        }

        [HttpGet("GetFileInfo")]
        public List<MyFileInfo> GetFileInfo()
        {
            var fileInfoToSend = new List<MyFileInfo>();
            
            const string path = "wwwroot\\data";
            string[] fileNames = Directory.GetFiles(path);

            foreach (var fileName in fileNames)
            {
                var myFileInfo = new MyFileInfo();
                var fileInfo = new FileInfo(fileName);
                myFileInfo.Name = fileInfo.Name;
                myFileInfo.Size = fileInfo.Length;
                myFileInfo.DateTimeUpload = fileInfo.CreationTime;
                myFileInfo.TypeOfFile = fileInfo.Extension;
                fileInfoToSend.Add(myFileInfo);
            }

            return fileInfoToSend;
        }

        [RequestSizeLimit(1024 * 1024 * 1024)]
        [HttpPost("GetFile")]
        public VirtualFileResult GetVirtualFile([FromBody] string fileName)
        {
            string filepath = Path.Combine("~/data", fileName);
            return File(filepath, "application/octet-stream", fileName);
        }

        [HttpDelete("DeleteOneFile")]
        public IActionResult DeleteOneFile([FromBody] string fileName)
        {
            string filepath = Path.Combine("wwwroot\\data", fileName);
            System.IO.File.Delete(filepath);
            return Ok();
        }

        [HttpDelete("DeleteAllFiles")]
        public IActionResult DeleteAllFiles()
        {
            var dirInfo = new DirectoryInfo("wwwroot\\data\\");
            foreach (var file in dirInfo.GetFiles())
            {
                file.Delete();
            }

            return Ok();
        }

        [HttpGet("GetMemorySize")]
        public long GetMemorySize()
        {
            var dirInfo = new DirectoryInfo("wwwroot\\data\\");
            return dirInfo.GetFiles().Sum(file => file.Length);
        }
    }
}
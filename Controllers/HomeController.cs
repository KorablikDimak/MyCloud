using System.Collections.Generic;
using System.IO;
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
                await using var stream = System.IO.File.Create($"wwwroot\\data\\{file.FileName}");
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
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
            try
            {
                foreach (var file in files)
                {
                    await using var stream = System.IO.File.Create($"wwwroot\\data\\{file.FileName}");
                    await file.CopyToAsync(stream);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            
            return View();
        }
    }
}
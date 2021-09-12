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
    }
}
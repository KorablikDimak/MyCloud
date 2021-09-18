using Microsoft.AspNetCore.Mvc;

namespace MyCloud.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        
        [HttpGet]
        public IActionResult Registration()
        {
            return View();
        }
    }
}
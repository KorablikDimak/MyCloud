using Microsoft.AspNetCore.Mvc;

namespace MyCloud.Controllers
{
    public class LoginController : Controller
    {
        [HttpGet]
        public IActionResult Registration()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
    }
}
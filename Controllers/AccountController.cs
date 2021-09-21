using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyCloud.DataBase;
using MyCloud.Models.Login;

namespace MyCloud.Controllers
{
    public class AccountController : Controller
    {
        private readonly IDatabaseRequest _databaseRequest;

        public AccountController(DataContext context)
        {
            _databaseRequest = new DatabaseRequest(context);
        }
        
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginModel loginModel)
        {
            if (!ModelState.IsValid) return new ForbidResult();

            var user = await _databaseRequest.FindUserAsync(loginModel.UserName, loginModel.Password);
            if (user == null) return new ForbidResult();
            
            await AuthenticateAsync(loginModel.UserName);
            return Ok();
        }
        
        private async Task AuthenticateAsync(string userName)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, userName)
            };
            
            var id = new ClaimsIdentity(claims, "ApplicationCookie",
                ClaimsIdentity.DefaultNameClaimType, 
                ClaimsIdentity.DefaultRoleClaimType);
            
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }
        
        [HttpGet]
        public IActionResult Registration()
        {
            return View();
        }

        [HttpPost("Registration")]
        public async Task<IActionResult> Registration([FromBody] RegistrationModel registrationModel)
        {
            if (!ModelState.IsValid) return new ForbidResult();
            if (!CreateUserDirectory(registrationModel.UserName)) return new ConflictResult();
            bool isAdded = await _databaseRequest.AddUserAsync(registrationModel.UserName, registrationModel.Password);
            if (!isAdded) return new ConflictResult();

            return Ok();
        }
        
        private bool CreateUserDirectory(string userName)
        {
            if (Directory.Exists($"wwwroot\\data\\{userName}")) return false;
            Directory.CreateDirectory($"wwwroot\\data\\{userName}");
            return true;
        }

        [Authorize]
        [HttpGet]
        public IActionResult Profile()
        {
            return View();
        }

        [Authorize]
        [HttpDelete("DeleteAccount")]
        public async Task<IActionResult> DeleteAccount([FromBody] LoginModel loginModel)
        {
            if (User.Identity == null) return new ConflictResult();
            bool isDeleted = await _databaseRequest.DeleteUserAsync(loginModel.UserName, loginModel.Password);
            if (!isDeleted) return new ConflictResult();
            if (!DeleteUserDirectory(User.Identity.Name)) return new ConflictResult();
            return Ok();
        }

        private bool DeleteUserDirectory(string userName)
        {
            var directoryInfo = new DirectoryInfo($"~\\data\\{userName}");

            if (!directoryInfo.Exists) return false;
            
            directoryInfo.Delete();
            return true;
        }
    }
}
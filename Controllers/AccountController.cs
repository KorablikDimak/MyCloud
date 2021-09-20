using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyCloud.Models.Login;
using MyCloud.Models.User;

namespace MyCloud.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserContext _databaseContext;

        public AccountController(UserContext context)
        {
            _databaseContext = context;
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
            
            var user = await _databaseContext.Users.FirstOrDefaultAsync(userData =>
                userData.UserName == loginModel.UserName && 
                userData.Password == loginModel.Password);

            if (user == null) return new ForbidResult();
            
            await AuthenticateAsync(loginModel.UserName);
            return Ok();
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
            
            var user = await _databaseContext.Users.FirstOrDefaultAsync(userData =>
                userData.UserName == registrationModel.UserName);

            if (user != null) return new ConflictResult();
            
            _databaseContext.Add(new User 
            { 
                UserName = registrationModel.UserName, 
                Password = registrationModel.Password
            });
            await _databaseContext.SaveChangesAsync();
            
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
    }
}
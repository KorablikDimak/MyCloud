using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyCloud.DataBase;
using MyCloud.DataBase.Interfaces;
using MyCloud.Models.Login;
using MyCloud.Models.User;

namespace MyCloud.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAdderIntoDatabase _adderIntoDatabase;
        private readonly IChangerDataInDatabase _changerDataInDatabase;
        private readonly IDeleterFromDatabase _deleterFromDatabase;
        private readonly IFinderFromDatabase _finderFromDatabase;

        public AccountController(DataContext context)
        {
            DatabaseRequest databaseRequest = new DatabaseRequest(context);
            _adderIntoDatabase = databaseRequest;
            _changerDataInDatabase = databaseRequest;
            _deleterFromDatabase = databaseRequest;
            _finderFromDatabase = databaseRequest;
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

            var user = await _finderFromDatabase.FindUserAsync(loginModel.UserName, loginModel.Password);
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
            bool isAdded = await _adderIntoDatabase.AddUserAsync(registrationModel.UserName, registrationModel.Password);
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
        [HttpPatch("ChangeUserName")]
        public async Task<IActionResult> ChangePersonality([FromBody] string newUserName, PersonalityData newPersonality)
        {
            if (User.Identity == null) return new ConflictResult();
            bool isChanged = await _changerDataInDatabase.ChangePersonalityDataAsync(User.Identity.Name, newUserName, newPersonality);
            if (isChanged) return Ok();
            return new ConflictResult();
        }

        [Authorize]
        [HttpPatch("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel passwordModel)
        {
            if (User.Identity == null) return new ConflictResult();
            bool isChanged = await _changerDataInDatabase.ChangePasswordAsync(
                User.Identity.Name, 
                passwordModel.OldPassword, 
                passwordModel.NewPassword);
            if (!isChanged) return new ConflictResult();
            return Ok();
        }
        
        [Authorize]
        [HttpDelete("DeleteAccount")]
        public async Task<IActionResult> DeleteAccount([FromBody] LoginModel loginModel)
        {
            if (User.Identity == null) return new ConflictResult();
            bool isDeleted = await _deleterFromDatabase.DeleteUserAsync(loginModel.UserName, loginModel.Password);
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
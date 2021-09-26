using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyCloud.DataBase.DataRequestBuilder;
using MyCloud.Models.Login;
using MyCloud.Models.User;

namespace MyCloud.Controllers
{
    public class AccountController : Controller
    {
        private readonly IDatabaseRequest _databaseRequest;

        public AccountController(AccountDataRequestBuilder accountDataRequestBuilder)
        {
            _databaseRequest = accountDataRequestBuilder.DatabaseRequest;
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

            var user = await _databaseRequest.DatabaseUsersRequest.FindUserAsync(loginModel.UserName, loginModel.Password);
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
            if (registrationModel.Password != registrationModel.ConfirmPassword) return new ConflictResult();
            
            bool isAdded = await _databaseRequest.DatabaseUsersRequest.AddUserAsync(registrationModel.UserName, registrationModel.Password);
            if (!isAdded) return new ConflictResult();
            
            bool isCreated = CreateUserDirectory(registrationModel.UserName);
            if (isCreated) return Ok();
            
            await _databaseRequest.DatabaseUsersRequest.DeleteUserAsync(registrationModel.UserName, registrationModel.Password);
            return new ConflictResult();
        }
        
        private bool CreateUserDirectory(string userName)
        {
            if (Directory.Exists($"UserFiles\\{userName}")) return false;
            Directory.CreateDirectory($"UserFiles\\{userName}");
            return true;
        }

        [Authorize]
        [HttpGet]
        public IActionResult Profile()
        {
            return View();
        }

        [Authorize]
        [HttpGet("GetPersonality")]
        public async Task<Personality> GetPersonality()
        {
            return await _databaseRequest.DatabasePersonalityRequest.FindPersonalityAsync(User.Identity.Name);
        }

        [Authorize]
        [HttpPatch("ChangeUserName")]
        public async Task<IActionResult> ChangeUserName([FromBody] string newUserName)
        {
            Personality personality = await _databaseRequest.DatabasePersonalityRequest.FindPersonalityAsync(User.Identity.Name);
            if (personality == null) return new ConflictResult();
            
            bool isUserNameChanged = await _databaseRequest.DatabaseUsersRequest.ChangeUserNameAsync(personality, newUserName);
            if (!isUserNameChanged) return new ConflictResult();
            
            bool isDirectoryChanged = ChangeDirectoryName(User.Identity.Name, newUserName);
            if (!isDirectoryChanged) return new ConflictResult();
            
            return Ok();
        }

        private bool ChangeDirectoryName(string oldUserName, string newUserName)
        {
            try
            {
                var directoryInfo = new DirectoryInfo($"UserFiles\\{oldUserName}");
                if (!directoryInfo.Exists) return false;
                directoryInfo.MoveTo($"UserFiles\\{newUserName}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            return true;
        }

        [Authorize]
        [HttpPatch("ChangePersonality")]
        public async Task<IActionResult> ChangePersonality([FromBody] Personality newPersonality)
        {
            bool isChanged = await _databaseRequest.DatabasePersonalityRequest.ChangePersonalityAsync(User.Identity.Name, newPersonality);
            if (isChanged) return Ok();
            return new ConflictResult();
        }

        [Authorize]
        [HttpPatch("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel passwordModel)
        {
            bool isChanged = await _databaseRequest.DatabaseUsersRequest.ChangePasswordAsync(
                User.Identity.Name, 
                passwordModel.OldPassword, 
                passwordModel.NewPassword);
            if (!isChanged) return new ConflictResult();
            return Ok();
        }
        
        [Authorize]
        [HttpDelete("DeleteAccount")]
        public async Task<IActionResult> DeleteAccount([FromBody] string password)
        {
            bool isUserDeleted = await _databaseRequest.DatabaseUsersRequest.DeleteUserAsync(User.Identity.Name, password);
            if (!isUserDeleted) return new ConflictResult();
            bool isDirectoryDeleted = DeleteUserDirectory(User.Identity.Name);
            if (!isDirectoryDeleted) return new ConflictResult();
            return Ok();
        }

        private bool DeleteUserDirectory(string userName)
        {
            try
            {
                var directoryInfo = new DirectoryInfo($"UserFiles\\{userName}");
                if (!directoryInfo.Exists) return false;
                directoryInfo.Delete();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }
            
            return true;
        }

        [Authorize]
        [HttpGet]
        public IActionResult MyGroups()
        {
            return View();
        }

        [Authorize]
        [HttpGet("FindMyGroups")]
        public async Task<List<GroupLogin>> FindMyGroups()
        {
            return new List<GroupLogin>(await _databaseRequest.DatabaseGroupsRequest.FindGroupsInUser(User.Identity.Name));
        }

        [Authorize]
        [HttpPost("FindUsersInGroup")]
        public async Task<List<Personality>> FindUsersInGroup([FromBody] GroupLogin groupLogin)
        {
            List<Personality> personalities = new List<Personality>();
            List<User> users = await _databaseRequest.DatabaseUsersRequest.FindUsersInGroup(groupLogin);
            foreach (var user in users)
            {
                Personality personality = await _databaseRequest.DatabasePersonalityRequest.FindPersonalityAsync(user.UserName);
                personalities.Add(personality);
            }

            return personalities;
        }

        [Authorize]
        [HttpPost("EnterInGroup")]
        public async Task<IActionResult> EnterInGroup([FromBody] GroupLogin groupLogin)
        {
            User user = await _databaseRequest.DatabaseUsersRequest.FindUserAsync(User.Identity.Name);
            if (user == null) return new ConflictResult();
            bool isEntered = await _databaseRequest.DatabaseGroupsRequest.AddUserInGroupAsync(groupLogin, user);
            if (isEntered) return Ok();
            return new ConflictResult();
        }

        [Authorize]
        [HttpDelete("LeaveFromGroup")]
        public async Task<IActionResult> LeaveFromGroup([FromBody] GroupLogin groupLogin)
        {
            User user = await _databaseRequest.DatabaseUsersRequest.FindUserAsync(User.Identity.Name);
            if (user == null) return new ConflictResult();
            bool isLeave = await _databaseRequest.DatabaseGroupsRequest.RemoveUserFromGroupAsync(groupLogin, user);
            if (isLeave) return Ok();
            return new ConflictResult();
        }

        [Authorize]
        [HttpPost("CreateGroup")]
        public async Task<IActionResult> CreateGroup([FromBody] GroupLogin groupLogin)
        {
            User user = await _databaseRequest.DatabaseUsersRequest.FindUserAsync(User.Identity.Name);
            if (user == null) return new ConflictResult();
            bool isCreated = await _databaseRequest.DatabaseGroupsRequest.CreateGroupAsync(groupLogin, user);
            if (isCreated) return Ok();
            return new ConflictResult();
        }

        [Authorize]
        [HttpDelete("DeleteGroup")]
        public async Task<IActionResult> DeleteGroup([FromBody] GroupLogin groupLogin)
        {
            bool isDeleted = await _databaseRequest.DatabaseGroupsRequest.DeleteGroupAsync(groupLogin);
            if (isDeleted) return Ok();
            return new ConflictResult();
        }

        [Authorize]
        [HttpPatch("ChangeGroupName")]
        public async Task<IActionResult> ChangeGroupLogin([FromBody] GroupLogin[] groupLogin)
        {
            bool isChanged = await _databaseRequest.DatabaseGroupsRequest.ChangeGroupLoginAsync(groupLogin[0], groupLogin[1]);
            if (isChanged) return Ok();
            return new ConflictResult();
        }
    }
}
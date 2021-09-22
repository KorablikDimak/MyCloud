using System;
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
        private readonly IDatabaseUsersRequest _databaseUsersRequest;
        private readonly IDatabasePersonalityRequest _databasePersonalityRequest;
        private readonly IDatabaseGroupsRequest _databaseGroupsRequest;
        private delegate Task<bool> ChangeDelegate(Group group, string newOption);

        public AccountController(DataContext context)
        {
            DatabaseRequest databaseRequest = new DatabaseRequest(context);
            DatabaseGroupRequest databaseGroupRequest = new DatabaseGroupRequest(context);
            _databaseUsersRequest = databaseRequest;
            _databasePersonalityRequest = databaseRequest;
            _databaseGroupsRequest = databaseGroupRequest;
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

            var user = await _databaseUsersRequest.FindUserAsync(loginModel.UserName, loginModel.Password);
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
            
            bool isAdded = await _databaseUsersRequest.AddUserAsync(registrationModel.UserName, registrationModel.Password);
            if (!isAdded) return new ConflictResult();
            
            bool isCreated = CreateUserDirectory(registrationModel.UserName);
            if (isCreated) return Ok();
            
            await _databaseUsersRequest.DeleteUserAsync(registrationModel.UserName, registrationModel.Password);
            return new ConflictResult();
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
        [HttpGet("GetPersonality")]
        public async Task<Personality> GetPersonality()
        {
            if (User.Identity == null) return new Personality();
            return await _databasePersonalityRequest.FindPersonalityAsync(User.Identity.Name);
        }

        [Authorize]
        [HttpPatch("ChangeUserName")]
        public async Task<IActionResult> ChangeUserName([FromBody] string newUserName)
        {
            if (User.Identity == null) return new ConflictResult();
            
            bool isUserNameChanged = await _databaseUsersRequest.ChangeUserNameAsync(User.Identity.Name, newUserName);
            if (!isUserNameChanged) return new ConflictResult();
            
            bool isDirectoryChanged = ChangeDirectoryName(User.Identity.Name, newUserName);
            if (!isDirectoryChanged) return new ConflictResult();
            
            return Ok();
        }

        private bool ChangeDirectoryName(string oldUserName, string newUserName)
        {
            try
            {
                var directoryInfo = new DirectoryInfo($"wwwroot\\data\\{oldUserName}");
                if (!directoryInfo.Exists) return false;
                directoryInfo.MoveTo($"wwwroot\\data\\{newUserName}");
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
            if (User.Identity == null) return new ConflictResult();
            bool isChanged = await _databasePersonalityRequest.ChangePersonalityAsync(User.Identity.Name, newPersonality);
            if (isChanged) return Ok();
            return new ConflictResult();
        }

        [Authorize]
        [HttpPatch("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel passwordModel)
        {
            if (User.Identity == null) return new ConflictResult();
            bool isChanged = await _databaseUsersRequest.ChangePasswordAsync(
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
            if (User.Identity == null) return new ConflictResult();
            bool isUserDeleted = await _databaseUsersRequest.DeleteUserAsync(User.Identity.Name, password);
            if (!isUserDeleted) return new ConflictResult();
            bool isDirectoryDeleted = DeleteUserDirectory(User.Identity.Name);
            if (!isDirectoryDeleted) return new ConflictResult();
            return Ok();
        }

        private bool DeleteUserDirectory(string userName)
        {
            try
            {
                var directoryInfo = new DirectoryInfo($"wwwroot\\data\\{userName}");
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
        [HttpPost("EnterInGroup")]
        public async Task<IActionResult> EnterInGroup([FromBody] GroupLogin groupLogin)
        {
            if (User.Identity == null) return new ConflictResult();
            User user = await _databaseUsersRequest.FindUserAsync(User.Identity.Name);
            if (user == null) return new ConflictResult();
            Group group = await _databaseGroupsRequest.FindGroup(groupLogin);
            if (group == null) return new ConflictResult();
            bool isEntered = await _databaseGroupsRequest.AddUserInGroup(user, group);
            if (isEntered) return Ok();
            return new ConflictResult();
        }

        [Authorize]
        [HttpDelete("LeaveFromGroup")]
        public async Task<IActionResult> LeaveFromGroup([FromBody] string groupName)
        {
            if (User.Identity == null) return new ConflictResult();
            User user = await _databaseUsersRequest.FindUserAsync(User.Identity.Name);
            if (user == null) return new ConflictResult();
            Group group = await _databaseGroupsRequest.FindGroup(groupName);
            if (group == null) return new ConflictResult();
            bool isLeave = await _databaseGroupsRequest.RemoveUserFromGroup(user, group);
            if (isLeave) return Ok();
            return new ConflictResult();
        }

        [Authorize]
        [HttpPost("CreateGroup")]
        public async Task<IActionResult> CreateGroup([FromBody] GroupLogin groupLogin)
        {
            if (User.Identity == null) return new ConflictResult();
            User user = await _databaseUsersRequest.FindUserAsync(User.Identity.Name);
            if (user == null) return new ConflictResult();
            Group group = await _databaseGroupsRequest.FindGroup(groupLogin.GroupName);
            if (group != null) return new ConflictResult();
            bool isCreated = await _databaseGroupsRequest.CreateGroup(groupLogin, user);
            if (isCreated) return Ok();
            return new ConflictResult();
        }

        [Authorize]
        [HttpDelete("DeleteGroup")]
        public async Task<IActionResult> DeleteGroup([FromBody] GroupLogin groupLogin)
        {
            if (User.Identity == null) return new ConflictResult();
            Group group = await _databaseGroupsRequest.FindGroup(groupLogin);
            if (group == null) return new ConflictResult();
            bool isDeleted = await _databaseGroupsRequest.DeleteGroup(group);
            if (isDeleted) return Ok();
            return new ConflictResult();
        }

        [Authorize]
        [HttpPatch("ChangeGroupName")]
        public async Task<IActionResult> ChangeGroupName([FromBody] GroupLoginChange newGroupLogin)
        {
            ChangeDelegate changeDelegate = _databaseGroupsRequest.ChangeGroupName;
            return await ChangeGroupLogin(changeDelegate, newGroupLogin);
        }

        [Authorize]
        [HttpPatch("ChangeGroupPassword")]
        public async Task<IActionResult> ChangeGroupPassword([FromBody] GroupLoginChange newGroupLogin)
        {
            ChangeDelegate changeDelegate = _databaseGroupsRequest.ChangeGroupPassword;
            return await ChangeGroupLogin(changeDelegate, newGroupLogin);
        }

        private async Task<IActionResult> ChangeGroupLogin(ChangeDelegate changeDelegate, GroupLoginChange newGroupLogin)
        {
            if (User.Identity == null) return new ConflictResult();
            Group group = await _databaseGroupsRequest.FindGroup(newGroupLogin);
            if (group == null) return new ConflictResult();
            bool isChanged = await changeDelegate(group, newGroupLogin.NewOption);
            if (isChanged) return Ok();
            return new ConflictResult();
        }
    }
}
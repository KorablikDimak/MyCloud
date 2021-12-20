using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyCloud.DataBase.DataRequestBuilder;
using MyCloud.Models.Login;
using MyCloud.Models.User;

namespace MyCloud.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly IDatabaseRequest _databaseRequest;

        public AccountController(AccountDataRequestBuilder accountDataRequestBuilder)
        {
            _databaseRequest = accountDataRequestBuilder.DatabaseRequest;
        }
        
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
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
        
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Registration()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost("Registration")]
        public async Task<IActionResult> Registration([FromBody] RegistrationModel registrationModel)
        {
            if (!ModelState.IsValid) return new ForbidResult();
            
            bool isAdded = await _databaseRequest.DatabaseUsersRequest.AddUserAsync(registrationModel.UserName, registrationModel.Password);
            if (!isAdded) return new ConflictResult();
            
            bool isCreated = CreateDirectory($"UserFiles\\{registrationModel.UserName}");
            if (isCreated) return Ok();
            
            await _databaseRequest.DatabaseUsersRequest.DeleteUserAsync(registrationModel.UserName, registrationModel.Password);
            return new ConflictResult();
        }

        [AllowAnonymous]
        [HttpPost("IsUserNameUsed")]
        public async Task<bool> IsUserNameUsed([FromBody] string userName)
        {
            User user = await _databaseRequest.DatabaseUsersRequest.FindUserAsync(userName);
            return user != null;
        }
        
        private bool CreateDirectory(string path)
        {
            if (Directory.Exists(path)) return false;
            Directory.CreateDirectory(path);
            return true;
        }
        
        [HttpGet]
        public IActionResult Profile()
        {
            return View();
        }
        
        [HttpGet("GetPersonality")]
        public async Task<Personality> GetPersonality()
        {
            return await _databaseRequest.DatabasePersonalityRequest.FindPersonalityAsync(User.Identity.Name);
        }
        
        [HttpPatch("ChangeUserName")]
        public async Task<IActionResult> ChangeUserName([FromBody][StringLength(20, MinimumLength = 3)] string newUserName)
        {
            if (!ModelState.IsValid) return new ForbidResult();
            
            Personality personality = await _databaseRequest.DatabasePersonalityRequest.FindPersonalityAsync(User.Identity.Name);
            if (personality == null) return new ConflictResult();
            
            bool isUserNameChanged = await _databaseRequest.DatabaseUsersRequest.ChangeUserNameAsync(personality, newUserName);
            if (!isUserNameChanged) return new ConflictResult();
            
            bool isDirectoryChanged = ChangeDirectoryName("UserFiles", User.Identity.Name, newUserName);
            if (!isDirectoryChanged) return new ConflictResult();
            
            return Ok();
        }

        private bool ChangeDirectoryName(string path, string oldName, string newName)
        {
            try
            {
                var directoryInfo = new DirectoryInfo($"{path}\\{oldName}");
                if (!directoryInfo.Exists) return false;
                directoryInfo.MoveTo($"{path}\\{newName}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            return true;
        }
        
        [HttpPatch("ChangePersonality")]
        public async Task<IActionResult> ChangePersonality([FromBody] Personality newPersonality)
        {
            if (!ModelState.IsValid) return new ForbidResult();
            bool isChanged = await _databaseRequest.DatabasePersonalityRequest
                .ChangePersonalityAsync(User.Identity.Name, newPersonality);
            if (isChanged) return Ok();
            return new ConflictResult();
        }
        
        [HttpPatch("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel passwordModel)
        {
            if (!ModelState.IsValid) return new ForbidResult();
            bool isChanged = await _databaseRequest.DatabaseUsersRequest.ChangePasswordAsync(
                User.Identity.Name, 
                passwordModel.OldPassword, 
                passwordModel.NewPassword);
            if (!isChanged) return new ConflictResult();
            return Ok();
        }
        
        [HttpDelete("DeleteAccount")]
        public async Task<IActionResult> DeleteAccount([FromBody][StringLength(20, MinimumLength = 3)] string password)
        {
            if (!ModelState.IsValid) return new ForbidResult();
            bool isUserDeleted = await _databaseRequest.DatabaseUsersRequest
                .DeleteUserAsync(User.Identity.Name, password);
            if (!isUserDeleted) return new ConflictResult();
            bool isDirectoryDeleted = DeleteDirectory($"UserFiles\\{User.Identity.Name}");
            if (!isDirectoryDeleted) return new ConflictResult();
            return Ok();
        }

        private bool DeleteDirectory(string path)
        {
            try
            {
                var directoryInfo = new DirectoryInfo(path);
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
        
        [HttpGet]
        public IActionResult MyGroups()
        {
            return View();
        }
        
        [HttpGet("FindMyGroups")]
        public async Task<List<GroupLogin>> FindMyGroups()
        {
            List<GroupLogin> groups = 
                new List<GroupLogin>(await _databaseRequest.DatabaseGroupsRequest.FindGroupsInUser(User.Identity.Name));
            return groups;
        }
        
        [HttpPost("FindUsersInGroup")]
        public async Task<List<Personality>> FindUsersInGroup([FromBody] GroupLogin groupLogin)
        {
            if (!ModelState.IsValid) return new List<Personality>();
            List<Personality> personalities = new List<Personality>();
            List<User> users = await _databaseRequest.DatabaseUsersRequest.FindUsersInGroup(groupLogin);
            foreach (var user in users)
            {
                Personality personality = await _databaseRequest.DatabasePersonalityRequest
                    .FindPersonalityAsync(user.UserName);
                personalities.Add(personality);
            }

            return personalities;
        }
        
        [HttpPost("EnterInGroup")]
        public async Task<IActionResult> EnterInGroup([FromBody] GroupLogin groupLogin)
        {
            if (!ModelState.IsValid) return new ForbidResult();
            User user = await _databaseRequest.DatabaseUsersRequest.FindUserAsync(User.Identity.Name);
            if (user == null) return new ConflictResult();
            bool isEntered = await _databaseRequest.DatabaseGroupsRequest.AddUserInGroupAsync(groupLogin, user);
            if (isEntered) return Ok();
            return new ConflictResult();
        }
        
        [HttpDelete("LeaveFromGroup")]
        public async Task<IActionResult> LeaveFromGroup([FromBody] GroupLogin groupLogin)
        {
            if (!ModelState.IsValid) return new ForbidResult();
            User user = await _databaseRequest.DatabaseUsersRequest.FindUserAsync(User.Identity.Name);
            if (user == null) return new ConflictResult();
            bool isLeave = await _databaseRequest.DatabaseGroupsRequest.RemoveUserFromGroupAsync(groupLogin, user);
            if (isLeave) return Ok();
            return new ConflictResult();
        }
        
        [HttpPost("CreateGroup")]
        public async Task<IActionResult> CreateGroup([FromBody] GroupLogin groupLogin)
        {
            if (!ModelState.IsValid) return new ForbidResult();
            bool isCreated = CreateDirectory($"CommonFiles\\{groupLogin.Name}");
            if (!isCreated) return new ConflictResult();
            User user = await _databaseRequest.DatabaseUsersRequest.FindUserAsync(User.Identity.Name);
            if (user == null) return new ConflictResult();
            isCreated = await _databaseRequest.DatabaseGroupsRequest.CreateGroupAsync(groupLogin, user);
            if (isCreated) return Ok();
            return new ConflictResult();
        }
        
        [HttpDelete("DeleteGroup")]
        public async Task<IActionResult> DeleteGroup([FromBody] GroupLogin groupLogin)
        {
            if (!ModelState.IsValid) return new ForbidResult();
            bool isDeleted = DeleteDirectory($"CommonFiles\\{groupLogin.Name}");
            if (!isDeleted) return new ConflictResult();
            isDeleted = await _databaseRequest.DatabaseGroupsRequest.DeleteGroupAsync(groupLogin);
            if (isDeleted) return Ok();
            return new ConflictResult();
        }
        
        [HttpPatch("ChangeGroupName")]
        public async Task<IActionResult> ChangeGroupLogin([FromBody] GroupLogin[] groupLogin)
        {
            if (!ModelState.IsValid) return new ForbidResult();
            bool isChanged = ChangeDirectoryName("CommonFiles", 
                groupLogin[0].Name, 
                groupLogin[1].Name);
            if (!isChanged) return new ConflictResult();
            isChanged = await _databaseRequest.DatabaseGroupsRequest
                .ChangeGroupLoginAsync(groupLogin[0], groupLogin[1]);
            if (isChanged) return Ok();
            return new ConflictResult();
        }
        
        [HttpPost("SetUserPhoto")]
        public async Task<IActionResult> SetUserPhoto(IFormFile image)
        {
            bool isDeleted = await DeleteOldIcon();
            if (!isDeleted) return new ConflictResult();
            bool isSet = await SetNewIcon(image);
            if (!isSet) return new ConflictResult();
            return Ok();
        }

        private async Task<bool> SetNewIcon(IFormFile image)
        {
            string iconName = $"{User.Identity.Name}.{image.FileName}";
            bool isSet = await _databaseRequest.DatabaseUsersRequest
                .SetIcon(User.Identity.Name, iconName);
            if (!isSet) return false;
            string filePath = $"wwwroot\\UserIcons\\{iconName}";
            await using var stream = System.IO.File.Create(filePath);
            await image.CopyToAsync(stream);
            return true;
        }

        private async Task<bool> DeleteOldIcon()
        {
            try
            {
                string iconName = await _databaseRequest.DatabaseUsersRequest.GetIcon(User.Identity.Name);
                if (!System.IO.File.Exists($"wwwroot\\UserIcons\\{iconName}")) return true;
                System.IO.File.Delete($"wwwroot\\UserIcons\\{iconName}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            return true;
        }
        
        [HttpGet("GetUserPhotoName")]
        public async Task<string> GetUserPhotoName()
        {
            string iconName = await _databaseRequest.DatabaseUsersRequest.GetIcon(User.Identity.Name);
            if (System.IO.File.Exists($"wwwroot\\UserIcons\\{iconName}"))
            {
                return $"UserIcons/{iconName}";
            }
            return "images/free-icon-user-149452.png";
        }
    }
}
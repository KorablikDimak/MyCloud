using System;
using System.Collections.Generic;
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
            
            bool isCreated = CreateDirectory($"UserFiles\\{registrationModel.UserName}");
            if (isCreated) return Ok();
            
            await _databaseRequest.DatabaseUsersRequest.DeleteUserAsync(registrationModel.UserName, registrationModel.Password);
            return new ConflictResult();
        }
        
        private bool CreateDirectory(string path)
        {
            if (Directory.Exists(path)) return false;
            Directory.CreateDirectory(path);
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

        [Authorize]
        [HttpPatch("ChangePersonality")]
        public async Task<IActionResult> ChangePersonality([FromBody] Personality newPersonality)
        {
            bool isChanged = await _databaseRequest.DatabasePersonalityRequest
                .ChangePersonalityAsync(User.Identity.Name, newPersonality);
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
            List<GroupLogin> groups = 
                new List<GroupLogin>(await _databaseRequest.DatabaseGroupsRequest.FindGroupsInUser(User.Identity.Name));
            return groups;
        }

        [Authorize]
        [HttpPost("FindUsersInGroup")]
        public async Task<List<Personality>> FindUsersInGroup([FromBody] GroupLogin groupLogin)
        {
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
            bool isCreated = CreateDirectory($"CommonFiles\\{groupLogin.Name}");
            if (!isCreated) return new ConflictResult();
            User user = await _databaseRequest.DatabaseUsersRequest.FindUserAsync(User.Identity.Name);
            if (user == null) return new ConflictResult();
            isCreated = await _databaseRequest.DatabaseGroupsRequest.CreateGroupAsync(groupLogin, user);
            if (isCreated) return Ok();
            return new ConflictResult();
        }

        [Authorize]
        [HttpDelete("DeleteGroup")]
        public async Task<IActionResult> DeleteGroup([FromBody] GroupLogin groupLogin)
        {
            bool isDeleted = DeleteDirectory($"CommonFiles\\{groupLogin.Name}");
            if (!isDeleted) return new ConflictResult();
            isDeleted = await _databaseRequest.DatabaseGroupsRequest.DeleteGroupAsync(groupLogin);
            if (isDeleted) return Ok();
            return new ConflictResult();
        }

        [Authorize]
        [HttpPatch("ChangeGroupName")]
        public async Task<IActionResult> ChangeGroupLogin([FromBody] GroupLogin[] groupLogin)
        {
            bool isChanged = ChangeDirectoryName("CommonFiles", 
                groupLogin[0].Name, 
                groupLogin[1].Name);
            if (!isChanged) return new ConflictResult();
            isChanged = await _databaseRequest.DatabaseGroupsRequest
                .ChangeGroupLoginAsync(groupLogin[0], groupLogin[1]);
            if (isChanged) return Ok();
            return new ConflictResult();
        }

        [Authorize]
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

        [Authorize]
        [HttpGet("GetUserPhotoUrl")]
        public async Task<string> GetUserPhotoUrl()
        {
            string iconName = await _databaseRequest.DatabaseUsersRequest.GetIcon(User.Identity.Name);
            if (System.IO.File.Exists($"wwwroot\\UserIcons\\{iconName}"))
            {
                return $"https://localhost:5001/UserIcons/{iconName}";
            }
            return "https://localhost:5001/images/free-icon-user-149452.png";
        }
    }
}
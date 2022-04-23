using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using InfoLog;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyCloud.DataBase.DataRequestBuilder;
using MyCloud.Models.Login;
using MyCloud.Models.Registration;
using MyCloud.Models.User;
using MyCloud.Security;
using Newtonsoft.Json;

namespace MyCloud.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private Repository Repository { get; }
        private ILogger Logger { get; }

        public AccountController(Repository repository, ILogger logger)
        {
            repository.ImplementLogger(logger);
            Repository = repository;
            Logger = logger;
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

            var user = await Repository.UsersRepository
                .FindUserAsync(loginModel.UserName, loginModel.Password.HashString());
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
            User user = await Repository.UsersRepository.FindUserAsync(registrationModel.UserName);
            if (user != null) return new ForbidResult();
            bool isSanded = await SendEmailToConfirm(registrationModel.UserName);
            if (!isSanded) return new ConflictResult();
            await Repository.RegistrationRepository.AddUserToConfirmAsync(new UserToConfirm
                { Email = registrationModel.UserName, Password = registrationModel.Password });
            return RedirectToAction("Login");
        }

        private async Task<bool> SendEmailToConfirm(string email)
        {
            using var client = new HttpClient();
            try
            {
                var siteLogin = new SiteLogin { SiteName = "MyCloud", Password = "asf791jas0f14rq3" };
                var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:5001/api/EmailSender/GetJwtToken")
                    { Content = CreateContent(siteLogin) };
                var response = await client.SendAsync(request);
                var jwtToken = await response.Content.ReadAsStringAsync();

                var emailRegistration = new EmailRegistrationData 
                    { EmailName = email, ResponseAddress = $"http://localhost:5001/ConfirmRegistration?token={email.HashString()}" };
                request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:5001/api/EmailSender/SendEmail")
                    { Content = CreateContent(emailRegistration) };
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer " + jwtToken);
                response = await client.SendAsync(request);
                if (!response.IsSuccessStatusCode) return false;
            }
            catch (Exception e)
            {
                await Logger.Warning(e.ToString());
                return false;
            }

            return true;
        }

        private HttpContent CreateContent(object o)
        {
            var json = JsonConvert.SerializeObject(o);
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        [AllowAnonymous]
        [HttpPost("ConfirmRegistration")]
        public async Task<IActionResult> ConfirmRegistration
            ([FromBody] RegistrationResponse registrationResponse, string token)
        {
            if (!ModelState.IsValid) return new ForbidResult();
            if (registrationResponse.Email.HashString() != token || !registrationResponse.IsConfirmed)
            {
                await Repository.RegistrationRepository.RemoveUserToConfirmAsync(registrationResponse.Email);
                return new ForbidResult();
            }
            
            UserToConfirm user = await Repository.RegistrationRepository
                .FindUserToConfirmAsync(registrationResponse.Email);
            if (user == null) return new NotFoundResult();
            bool isAdded = await Repository.UsersRepository
                .AddUserAsync(user.Email, user.Password);
            if (!isAdded)
            {
                await Repository.RegistrationRepository.RemoveUserToConfirmAsync(user.Email);
                return new ConflictResult();
            }
            
            bool isCreated = CreateDirectory($"UserFiles\\{user.Email}");
            if (isCreated)
            {
                await Repository.RegistrationRepository.RemoveUserToConfirmAsync(user.Email);
                return RedirectToAction("Login");
            }

            await Repository.UsersRepository
                .DeleteUserAsync(registrationResponse.Email, user.Email);
            await Repository.RegistrationRepository.RemoveUserToConfirmAsync(user.Email);
            return new ConflictResult();
        }

        [AllowAnonymous]
        [HttpPost("IsUserNameUsed")]
        public async Task<bool> IsUserNameUsed([FromBody] string userName)
        {
            User user = await Repository.UsersRepository.FindUserAsync(userName);
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
            return await Repository.PersonalityRepository.FindPersonalityAsync(User.Identity.Name);
        }
        
        [HttpPatch("ChangeUserName")]
        public async Task<IActionResult> ChangeUserName([FromBody][StringLength(20, MinimumLength = 3)] string newUserName)
        {
            if (!ModelState.IsValid) return new ForbidResult();
            
            Personality personality = await Repository.PersonalityRepository.FindPersonalityAsync(User.Identity.Name);
            if (personality == null) return new ConflictResult();
            
            bool isUserNameChanged = await Repository.UsersRepository.ChangeUserNameAsync(personality, newUserName);
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
            bool isChanged = await Repository.PersonalityRepository
                .ChangePersonalityAsync(User.Identity.Name, newPersonality);
            if (isChanged) return Ok();
            return new ConflictResult();
        }
        
        [HttpPatch("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel passwordModel)
        {
            if (!ModelState.IsValid) return new ForbidResult();
            bool isChanged = await Repository.UsersRepository.ChangePasswordAsync(
                User.Identity.Name, 
                passwordModel.OldPassword.HashString(), 
                passwordModel.NewPassword.HashString());
            if (!isChanged) return new ConflictResult();
            return Ok();
        }
        
        [HttpDelete("DeleteAccount")]
        public async Task<IActionResult> DeleteAccount([FromBody][StringLength(20, MinimumLength = 3)] string password)
        {
            if (!ModelState.IsValid) return new ForbidResult();
            bool isUserDeleted = await Repository.UsersRepository
                .DeleteUserAsync(User.Identity.Name, password.HashString());
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
        
        [AllowAnonymous]
        [HttpPost("IsGroupNameUsed")]
        public async Task<bool> IsGroupNameUsed([FromBody] string groupName)
        {
            Group group = await Repository.GroupsRepository.FindGroupAsync(groupName);
            return group != null;
        }
        
        [HttpGet("FindMyGroups")]
        public async Task<List<GroupLogin>> FindMyGroups()
        {
            List<GroupLogin> groups = 
                new List<GroupLogin>(await Repository.GroupsRepository.FindGroupsInUser(User.Identity.Name));
            return groups;
        }
        
        [HttpPost("FindUsersInGroup")]
        public async Task<List<Personality>> FindUsersInGroup([FromBody] string groupName)
        {
            if (!ModelState.IsValid) return new List<Personality>();
            List<Personality> personalities = new List<Personality>();
            List<User> users = await Repository.GroupsRepository.FindUsersInGroup(groupName);
            foreach (var user in users)
            {
                Personality personality = await Repository.PersonalityRepository
                    .FindPersonalityAsync(user.UserName);
                personalities.Add(personality);
            }

            return personalities;
        }
        
        [HttpPost("EnterInGroup")]
        public async Task<IActionResult> EnterInGroup([FromBody] GroupLogin groupLogin)
        {
            if (!ModelState.IsValid) return new ForbidResult();
            User user = await Repository.UsersRepository.FindUserAsync(User.Identity.Name);
            if (user == null) return new ConflictResult();
            groupLogin.GroupPassword = groupLogin.GroupPassword.HashString();
            bool isEntered = await Repository.GroupsRepository.AddUserInGroupAsync(groupLogin, user);
            if (isEntered) return Ok();
            return new ConflictResult();
        }
        
        [HttpDelete("LeaveFromGroup")]
        public async Task<IActionResult> LeaveFromGroup([FromBody] GroupLogin groupLogin)
        {
            if (!ModelState.IsValid) return new ForbidResult();
            User user = await Repository.UsersRepository.FindUserAsync(User.Identity.Name);
            if (user == null) return new ConflictResult();

            groupLogin.GroupPassword = groupLogin.GroupPassword.HashString();
            bool isLeave = await Repository.GroupsRepository.RemoveUserFromGroupAsync(groupLogin, user);
            if (!isLeave) return new ConflictResult();

            Group group = await Repository.GroupsRepository.FindGroupAsync(groupLogin);
            if (group != null) return Ok();
            
            bool isDeleted = DeleteDirectory($"CommonFiles\\{groupLogin.Name}");
            if (!isDeleted) return new ConflictResult();

            return Ok();
        }
        
        [HttpPost("CreateGroup")]
        public async Task<IActionResult> CreateGroup([FromBody] GroupLogin groupLogin)
        {
            if (!ModelState.IsValid) return new ForbidResult();
            User user = await Repository.UsersRepository.FindUserAsync(User.Identity.Name);
            if (user == null) return new ConflictResult();
            
            bool isCreated = CreateDirectory($"CommonFiles\\{groupLogin.Name}");
            if (!isCreated) return new ConflictResult();
            
            groupLogin.GroupPassword = groupLogin.GroupPassword.HashString();
            isCreated = await Repository.GroupsRepository.CreateGroupAsync(groupLogin, user);
            
            if (isCreated) return Ok();
            DeleteDirectory($"CommonFiles\\{groupLogin.Name}");
            return new ConflictResult();
        }
        
        [HttpDelete("DeleteGroup")]
        public async Task<IActionResult> DeleteGroup([FromBody] GroupLogin groupLogin)
        {
            if (!ModelState.IsValid) return new ForbidResult();
            groupLogin.GroupPassword = groupLogin.GroupPassword.HashString();
            bool isDeleted = await Repository.GroupsRepository.DeleteGroupAsync(groupLogin);
            if (!isDeleted) return new ConflictResult();
            
            isDeleted = DeleteDirectory($"CommonFiles\\{groupLogin.Name}");
            if (isDeleted) return Ok();
            
            User user = await Repository.UsersRepository.FindUserAsync(User.Identity.Name);
            await Repository.GroupsRepository.CreateGroupAsync(groupLogin, user);
            return new ConflictResult();
        }
        
        [HttpPatch("ChangeGroupLogin")]
        public async Task<IActionResult> ChangeGroupLogin([FromBody] GroupLogin[] groupLogin)
        {
            if (!ModelState.IsValid) return new ForbidResult();

            groupLogin[0].GroupPassword = groupLogin[0].GroupPassword.HashString();
            groupLogin[1].GroupPassword = groupLogin[1].GroupPassword.HashString();

            bool isChanged = await Repository.GroupsRepository
                .ChangeGroupLoginAsync(groupLogin[0], groupLogin[1]);
            if (!isChanged) return new ConflictResult();
            
            isChanged = ChangeDirectoryName("CommonFiles", 
                groupLogin[0].Name, 
                groupLogin[1].Name);
            if (isChanged) return Ok();
            
            await Repository.GroupsRepository
                .ChangeGroupLoginAsync(groupLogin[1], groupLogin[0]);
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
            bool isSet = await Repository.UsersRepository
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
                string iconName = await Repository.UsersRepository.GetIcon(User.Identity.Name);
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
            string iconName = await Repository.UsersRepository.GetIcon(User.Identity.Name);
            if (System.IO.File.Exists($"wwwroot\\UserIcons\\{iconName}"))
            {
                return $"UserIcons/{iconName}";
            }
            return "images/free-icon-user-149452.png";
        }
    }
}
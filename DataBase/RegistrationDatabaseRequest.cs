using System;
using System.Threading.Tasks;
using InfoLog;
using Microsoft.EntityFrameworkCore;
using MyCloud.DataBase.Interfaces;
using MyCloud.Models.Registration;

namespace MyCloud.DataBase
{
    public class RegistrationDatabaseRequest : IRegistrationRepository, IHaveLogger
    {
        private DataContext DatabaseContext { get; }
        public ILogger Logger { get; set; }

        public RegistrationDatabaseRequest(DataContext context)
        {
            DatabaseContext = context;
        }
        
        public async Task<UserToConfirm> FindUserToConfirmAsync(string currentEmail)
        { 
            return await DatabaseContext.Emails.FirstOrDefaultAsync(email => email.Email == currentEmail);
        }

        public async Task<bool> RemoveUserToConfirmAsync(string email)
        {
            try
            {
                UserToConfirm user = await FindUserToConfirmAsync(email);
                DatabaseContext.Remove(user);
                await DatabaseContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                await Logger?.Warning(e.ToString())!;
                return false;
            }

            return true;
        }

        public async Task<bool> AddUserToConfirmAsync(UserToConfirm user)
        {
            try
            {
                UserToConfirm userToConfirm = await FindUserToConfirmAsync(user.Email);
                if (userToConfirm != null) return false;
                await DatabaseContext.Emails.AddAsync(user);
                await DatabaseContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                await Logger?.Warning(e.ToString())!;
                return false;
            }

            return true;
        }
    }
}
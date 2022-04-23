using System.Threading.Tasks;
using MyCloud.Models.Registration;

namespace MyCloud.DataBase.Interfaces
{
    public interface IRegistrationRepository
    {
        public Task<UserToConfirm> FindUserToConfirmAsync(string email);
        public Task<bool> RemoveUserToConfirmAsync(string email);
        public Task<bool> AddUserToConfirmAsync(UserToConfirm user);
    }
}
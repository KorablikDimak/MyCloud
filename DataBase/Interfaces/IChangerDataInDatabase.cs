using System.Threading.Tasks;
using MyCloud.Models.User;

namespace MyCloud.DataBase.Interfaces
{
    public interface IChangerDataInDatabase
    {
        public Task<bool> ChangePersonalityDataAsync(string userName, Personality newPersonality);
        public Task<bool> ChangePasswordAsync(string userName, string oldPassword, string newPassword);
        public Task<bool> ChangeUserNameAsync(string oldUserName, string newUserName);
    }
}
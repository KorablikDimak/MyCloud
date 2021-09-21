using System.Threading.Tasks;
using MyCloud.Models.User;

namespace MyCloud.DataBase.Interfaces
{
    public interface IChangerDataInDatabase
    {
        public Task<bool> ChangePersonalityDataAsync(string oldUserName, string newUserName, PersonalityData newPersonality);
        public Task<bool> ChangePasswordAsync(string userName, string oldPassword, string newPassword);
    }
}
using System.Threading.Tasks;
using MyCloud.Models.User;

namespace MyCloud.DataBase.Interfaces
{
    public interface IPersonalityRepository
    {
        public Task<Personality> FindPersonalityAsync(string userName);
        public Task<bool> ChangePersonalityAsync(string userName, Personality newPersonality);
    }
}
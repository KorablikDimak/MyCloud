using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyCloud.DataBase.Interfaces;
using MyCloud.Models.User;

namespace MyCloud.DataBase
{
    public class DatabasePersonalityRequest : IDatabasePersonalityRequest
    {
        public DataContext DatabaseContext { private get; init; }

        public async Task<Personality> FindPersonalityAsync(string userName)
        {
            Personality personality = await DatabaseContext.Personality.FirstOrDefaultAsync(personalityData => 
                personalityData.User.UserName == userName);
            return personality;
        }
        
        public async Task<bool> ChangePersonalityAsync(string userName, Personality newPersonality)
        {
            try
            {
                Personality personality = await FindPersonalityAsync(userName);
                if (personality == null) return false;
                personality.Name = newPersonality.Name;
                personality.Surname = newPersonality.Surname;
                await DatabaseContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return false;
            }

            return true;
        }
    }
}
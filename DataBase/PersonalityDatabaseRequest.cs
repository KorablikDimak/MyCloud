using System;
using System.Threading.Tasks;
using InfoLog;
using Microsoft.EntityFrameworkCore;
using MyCloud.DataBase.Interfaces;
using MyCloud.Models.User;

namespace MyCloud.DataBase
{
    public class PersonalityDatabaseRequest : IPersonalityRepository, IHaveLogger
    {
        private DataContext DatabaseContext { get; }
        public ILogger Logger { get; set; }

        public PersonalityDatabaseRequest(DataContext context)
        {
            DatabaseContext = context;
        }

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
                await Logger?.Error(e.ToString())!;
                return false;
            }

            return true;
        }
    }
}
using System;
using System.Threading.Tasks;
using InfoLog;
using Microsoft.EntityFrameworkCore;
using MyCloud.DataBase.Interfaces;
using MyCloud.Models.User;

namespace MyCloud.DataBase
{
    public class DatabasePersonalityRequest : IDatabasePersonalityRequest, IHaveLogger
    {
        private readonly DataContext _databaseContext;
        public ILogger Logger { get; set; }

        public DatabasePersonalityRequest(DataContext context)
        {
            _databaseContext = context;
        }

        public async Task<Personality> FindPersonalityAsync(string userName)
        {
            Personality personality = await _databaseContext.Personality.FirstOrDefaultAsync(personalityData => 
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
                await _databaseContext.SaveChangesAsync();
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
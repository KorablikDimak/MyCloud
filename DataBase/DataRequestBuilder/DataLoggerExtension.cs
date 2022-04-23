using InfoLog;
using MyCloud.DataBase.Interfaces;

namespace MyCloud.DataBase.DataRequestBuilder
{
    public static class DataLoggerExtension
    {
        public static void ImplementLogger(this IHaveLogger current, ILogger logger)
        {
            if (current != null && logger != null)
            {
                current.Logger = logger;
            }
        }

        public static void ImplementLogger(this Repository repository, ILogger logger)
        {
            (repository.FilesRepository as IHaveLogger).ImplementLogger(logger);
            (repository.GroupsRepository as IHaveLogger).ImplementLogger(logger);
            (repository.PersonalityRepository as IHaveLogger).ImplementLogger(logger);
            (repository.UsersRepository as IHaveLogger).ImplementLogger(logger);
            (repository.CommonFilesRepository as IHaveLogger).ImplementLogger(logger);
            (repository.RegistrationRepository as IHaveLogger).ImplementLogger(logger);
        }
    }
}
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

        public static void ImplementLogger(this DatabaseRequest databaseRequest, ILogger logger)
        {
            (databaseRequest.DatabaseFilesRequest as IHaveLogger).ImplementLogger(logger);
            (databaseRequest.DatabaseGroupsRequest as IHaveLogger).ImplementLogger(logger);
            (databaseRequest.DatabasePersonalityRequest as IHaveLogger).ImplementLogger(logger);
            (databaseRequest.DatabaseUsersRequest as IHaveLogger).ImplementLogger(logger);
            (databaseRequest.DatabaseCommonFilesRequest as IHaveLogger).ImplementLogger(logger);
        }
    }
}
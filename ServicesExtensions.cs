using InfoLog;
using InfoLog.Config;
using Microsoft.Extensions.DependencyInjection;
using MyCloud.DataBase;
using MyCloud.DataBase.Interfaces;

namespace MyCloud
{
    public static class ServicesExtensions
    {
        public static void AddDataRequest<T>(this IServiceCollection services)
            where T : IDatabaseRequestBuilder, new()
        {
            services.AddScoped(provider =>
            {
                DataContext context = provider.GetService<DataContext>();
                T requestBuilder = new T { Context = context };
                requestBuilder.CreateDatabaseRequest();
                return requestBuilder.DatabaseRequest;
            });
        }

        public static void AddLogger<T>(this IServiceCollection services, string xmlPath)
            where T : ILogger, new()
        {
            services.AddSingleton(provider =>
            {
                var configuration = new Configuration(xmlPath);
                var loggerFactory = new LoggerFactory<T>(configuration);
                return loggerFactory.CreateLogger();
            });
        }
    }
}
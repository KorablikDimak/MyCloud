using Microsoft.Extensions.DependencyInjection;
using MyCloud.DataBase;
using MyCloud.DataBase.Interfaces;

namespace MyCloud
{
    public static class ServicesExtensions
    {
        public static void AddUserRequest<T>(this IServiceCollection services)
            where T : class, IDatabaseUsersRequest, new()
        {
            services.AddScoped<IDatabaseUsersRequest>(provider =>
            {
                DataContext context = provider.GetService<DataContext>();
                return new T { DatabaseContext = context };
            });
        }
        
        public static void AddGroupsRequest<T>(this IServiceCollection services)
            where T : class, IDatabaseGroupsRequest, new()
        {
            services.AddScoped<IDatabaseGroupsRequest>(provider =>
            {
                DataContext context = provider.GetService<DataContext>();
                return new T { DatabaseContext = context };
            });
        }
        
        public static void AddFilesRequest<T>(this IServiceCollection services) 
            where T : class, IDatabaseFilesRequest, new()
        {
            services.AddScoped<IDatabaseFilesRequest>(provider =>
            {
                DataContext context = provider.GetService<DataContext>();
                return new T { DatabaseContext = context };
            });
        }

        public static void AddPersonalityRequest<T>(this IServiceCollection services) 
            where T : class, IDatabasePersonalityRequest, new()
        {
            services.AddScoped<IDatabasePersonalityRequest>(provider =>
            {
                DataContext context = provider.GetService<DataContext>();
                return new T {DatabaseContext = context};
            });
        }
    }
}
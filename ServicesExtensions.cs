using Microsoft.Extensions.DependencyInjection;
using MyCloud.DataBase;
using MyCloud.DataBase.Interfaces;

namespace MyCloud
{
    public static class ServicesExtensions
    {
        public static void AddUserRequest(this IServiceCollection services)
        {
            services.AddScoped<IDatabaseUsersRequest>(provider =>
            {
                DataContext context = provider.GetService<DataContext>();
                return new DatabaseUsersRequest(context);
            });
        }

        public static void AddGroupsRequest(this IServiceCollection services)
        {
            services.AddScoped<IDatabaseGroupsRequest>(provider =>
            {
                DataContext context = provider.GetService<DataContext>();
                return new DatabaseGroupsRequest(context);
            });
        }

        public static void AddFilesRequest(this IServiceCollection services)
        {
            services.AddScoped<IDatabaseFilesRequest>(provider =>
            {
                DataContext context = provider.GetService<DataContext>();
                return new DatabaseFilesRequest(context);
            });
        }

        public static void AddPersonalityRequest(this IServiceCollection services)
        {
            services.AddScoped<IDatabasePersonalityRequest>(provider =>
            {
                DataContext context = provider.GetService<DataContext>();
                return new DatabasePersonalityRequest(context);
            });
        }
    }
}
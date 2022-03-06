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
    }
}
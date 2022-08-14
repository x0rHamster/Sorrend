using Microsoft.Extensions.DependencyInjection;

namespace Sorrend.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSorrendCore(this IServiceCollection services)
        {
            services.AddSingleton<ApplicationServices>();

            return services;
        }
    }
}

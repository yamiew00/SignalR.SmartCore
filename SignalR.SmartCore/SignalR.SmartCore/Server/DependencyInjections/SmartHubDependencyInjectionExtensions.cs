using Microsoft.AspNetCore.SignalR;
using SignalR.SmartCore.Server.Filters;
using SignalR.SmartCore.Server.Managers;
using SignalR.SmartCore.Server.Providers;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SmartHubDependencyInjectionExtensions
    {
        public static IServiceCollection AddSmartSignalR(this IServiceCollection services)
        {
            //add Filter
            services.AddSignalR(options =>
            {
                options.AddFilter<SmartManagerFilter>();
            });

            //all concrete SmartHub type
            var hubTypes = AppDomain.CurrentDomain
                                    .GetAssemblies()
                                    .SelectMany(s => s.GetTypes())
                                    .Where(type => !type.IsAbstract &&
                                           type.IsSubclassOf(typeof(Hub)));

            //add manager
            services.AddSingleton(serviceType: typeof(ISmartHubManager),
                                  implementationInstance: new SmartHubManager(hubTypes));

            //add non-generic hubProvider
            services.AddSingleton(serviceType: typeof(ISmartHubProvider),
                                  implementationType: typeof(SmartHubProvider));

            //add generic hubProvider
            foreach (var hubType in hubTypes)
            {
                services.AddSingleton(serviceType: typeof(ISmartHubProvider<>).MakeGenericType(hubType),
                                      implementationType: typeof(SmartHubProvider<>).MakeGenericType(hubType));
            }

            return services;
        }
    }
}

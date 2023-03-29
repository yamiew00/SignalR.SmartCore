using Microsoft.AspNetCore.SignalR;
using SignalR.SmartCore.Server;
using SignalR.SmartCore.Server.Attributes;
using SignalR.SmartCore.Server.DependencyInjections;
using SignalR.SmartCore.Server.DependencyInjections.Builders;
using SignalR.SmartCore.Server.Filters;
using SignalR.SmartCore.Server.Filters.Authenticators;
using SignalR.SmartCore.Server.Managers;
using SignalR.SmartCore.Server.Providers;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SmartHubDependencyInjectionExtensions
    {
        /// <summary>
        /// Adds SmartCore services to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <param name="configure">An <see cref="Action{SmartHubOptions}"/> to configure the provided <see cref="SmartHubOptions"/>.</param>
        /// <returns></returns>
        public static IServiceCollection AddSmartSignalR(this IServiceCollection services, Action<SmartHubOptions> configure = default)
        {
            var hubTypes = ServerGlobal.SmartHubConcreteTypes;

            //configure
            SmartHubOptions smartHubOptions = new SmartHubOptions();
            configure?.Invoke(smartHubOptions);
            Type authenticatorType = smartHubOptions.AuthenticatorType;

            //add Filter
            var signalRBuilder = services.AddSignalR(options =>
            {
                options.AddFilter<SmartManagerFilter>();
                options.EnableDetailedErrors = true;
            });

            //configure the SmartHubOptions
            if (configure != null)
            {
                // Register ISmartHubAuthenticator as authenticatorType. If authenticatorType is not specified, the default type will be DefaultAuthenticator.
                services.AddSingleton(serviceType: typeof(ISmartHubAuthenticator),
                                      implementationType: authenticatorType);

                foreach (var hubType in hubTypes)
                {
                    var isAnonymous = hubType.GetCustomAttribute(typeof(AllowHubAnonymousAttribute)) != null;
                    if (isAnonymous) continue;
                    signalRBuilder.AddAuthenticator(hubType);
                }
            }
            
            //add manager
            services.AddSingleton(serviceType: typeof(ISmartHubManager),
                                  implementationInstance: new SmartHubManager(ServerGlobal.SmartHubConcreteTypes));

            //add non-generic hubProvider
            services.AddSingleton(serviceType: typeof(ISmartHubProvider),
                                  implementationType: typeof(SmartHubProvider));

            //add generic hubProvider
            foreach (var hubType in ServerGlobal.SmartHubConcreteTypes)
            {
                services.AddSingleton(serviceType: typeof(ISmartHubProvider<>).MakeGenericType(hubType),
                                      implementationType: typeof(SmartHubProvider<>).MakeGenericType(hubType));
            }

            return services;
        }

        /// <summary>
        /// For every hubtype : SmartHub<IHubService>, it runs :
        /// signalRBuilder.AddHubOptions<HubType>(options =>
        ///               {
        ///                 options.AddFilter<AuthenticatorType<HubType, IHubService>>();
        ///               });
        ///               
        /// where AuthenticatorType is determined by options.
        /// </summary>
        /// <param name="signalRBuilder"></param>
        /// <param name="hubType"> hubType must be a SmartHub<GenericType> where GenericType: class</param>
        private static void AddAuthenticator(this ISignalRServerBuilder signalRBuilder, Type hubType)
        {
            var genericType = hubType.GetSmartHubGenericTypeParameter();

#pragma warning disable CS8602
            //get the 'AddHubOptions<HubType>' method in SignalRDependencyInjectionExtensions.
            MethodInfo genericAddHubOptionsMethod = Assembly.Load("Microsoft.AspNetCore.SignalR")
                                                            .GetType("Microsoft.Extensions.DependencyInjection.SignalRDependencyInjectionExtensions")
                                                            .GetMethod("AddHubOptions", BindingFlags.Static | BindingFlags.Public)
                                                            .MakeGenericMethod(hubType);
#pragma warning restore CS8602

            /* Create a delegate for adding the filter with the given type
               the delegate seems like: 
                option =>
                {
                    options.AddFilter<authenticatorType>();
                }
            */
            // Create a delegate for adding the filter with the given type
            var optionsParameter = Expression.Parameter(typeof(HubOptions<>).MakeGenericType(hubType), "options");
            var addFilterMethod = typeof(HubOptionsExtensions).GetMethod("AddFilter", BindingFlags.Public | BindingFlags.Static, null, new Type[] { optionsParameter.Type, typeof(Type) }, null);

            // Create the filter type with the specified generic arguments
            Type authenticationBaseFilterType = typeof(AuthenticationBaseFilter<,>).MakeGenericType(hubType, genericType);
            var addFilterCall = Expression.Call(addFilterMethod, optionsParameter, Expression.Constant(authenticationBaseFilterType));

            var lambda = Expression.Lambda(typeof(Action<>).MakeGenericType(typeof(HubOptions<>).MakeGenericType(hubType)), addFilterCall, optionsParameter);
            Delegate configureDelegate = lambda.Compile();

            // invoke signalRBuilder.AddHubOptions<HubType>(action) where HubType is a runtimeType
            genericAddHubOptionsMethod.Invoke(null, new object[] { signalRBuilder, configureDelegate });
        }
    }
}

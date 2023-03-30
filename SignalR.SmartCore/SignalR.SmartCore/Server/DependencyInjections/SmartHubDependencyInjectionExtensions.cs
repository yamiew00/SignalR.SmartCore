using Microsoft.AspNetCore.SignalR;
using SignalR.SmartCore.Extensions;
using SignalR.SmartCore.Server;
using SignalR.SmartCore.Server.Attributes;
using SignalR.SmartCore.Server.DependencyInjections;
using SignalR.SmartCore.Server.DependencyInjections.Builders;
using SignalR.SmartCore.Server.Filters;
using SignalR.SmartCore.Server.Filters.Authenticators;
using SignalR.SmartCore.Server.Filters.SmartHubFilters;
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
        /// <param name="configure">An <see cref="Action{SmartHubOptions}"/> to configure the provided <see cref="SmartCoreOptions"/>.</param>
        /// <returns></returns>
        public static IServiceCollection AddSmartSignalR(this IServiceCollection services, Action<SmartCoreOptions> configure = default)
        {
            var hubTypes = ServerGlobal.SmartHubConcreteTypes;

            //configure
            SmartCoreOptions smartCoreOptions = new SmartCoreOptions();
            configure?.Invoke(smartCoreOptions);
            Type? authenticatorType = smartCoreOptions.AuthenticatorType;

            //add global Filter
            var signalRBuilder = services.AddSignalR(options =>
            {
                options.AddFilter<SmartManagerFilter>();
                options.EnableDetailedErrors = true;
            });

            //configure the SmartHubOptions
            if (authenticatorType != null)
            {
                // Register ISmartHubAuthenticator as authenticatorType. If authenticatorType is not specified, the default type will be DefaultAuthenticator.
                services.AddScoped(serviceType: typeof(ISmartHubAuthenticator),
                                   implementationType: authenticatorType);

                foreach (var hubType in hubTypes)
                {
                    var isAnonymous = hubType.GetCustomAttribute(typeof(AllowHubAnonymousAttribute)) != null;
                    if (isAnonymous) continue;
                    signalRBuilder.AddAuthenticator(hubType);
                }
            }
            //add local Filter
            if (smartCoreOptions.SmartHubOptionsDict.Any())
            {
                foreach (var optionValuePair in smartCoreOptions.SmartHubOptionsDict)
                {
                    var hubType = optionValuePair.Key;
                    var smartHubOption = optionValuePair.Value;

                    if (!smartHubOption.FilterTypes.Any()) continue;

                    MethodInfo addHubOptions_GenericMethod = AddHubOptions_GenericMethod.MakeGenericMethod(hubType);
                    List<Action<HubOptions>> hubOptionList = new List<Action<HubOptions>>();
                    foreach (var smartHubFilterBaseType in smartHubOption.FilterTypes)
                    {
                        services.AddScoped(smartHubFilterBaseType);
                        /*  For each implemented filterType, it is necessary to do
                            signalRBuilder.AddHubOptions<TSmartHub>(options =>
                            {
                                options.AddFilter<SmartHubFilterBase_IHubFilter_Adapter<TFilterType, TSmartHub>>();
                            });
                        */

                        // Construct a delegate for configuring HubOptions within AddHubOptions<THub>
                        void configureHubOptions(HubOptions options)
                        {
                            Type adapterType = typeof(SmartHubFilterBase_IHubFilter_Adapter<,>).MakeGenericType(smartHubFilterBaseType, hubType);

                            MethodInfo addFilterMethod = AddFilter_GenericMethod.MakeGenericMethod(adapterType);
                            addFilterMethod.Invoke(null, new object[] { options });
                        }
                        hubOptionList.Add(configureHubOptions);
                    }

                    Action<HubOptions> combinedConfigureHubOptions = options =>
                    {
                        foreach (var configureHubOptionsAction in hubOptionList)
                        {
                            configureHubOptionsAction(options);
                        }
                    };

                    // .AddHubOption<TSmartHub>(opt => opt.AddFilter()... );
                    addHubOptions_GenericMethod.Invoke(null, new object[] { signalRBuilder, combinedConfigureHubOptions });
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

            //get the 'AddHubOptions<HubType>' method in SignalRDependencyInjectionExtensions.
            MethodInfo addHubOptions_GenericMethod = AddHubOptions_GenericMethod.MakeGenericMethod(hubType);

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
            addHubOptions_GenericMethod.Invoke(null, new object[] { signalRBuilder, configureDelegate });
        }

#pragma warning disable CS8601, CS8602 
        private static readonly MethodInfo AddHubOptions_GenericMethod = Assembly.Load("Microsoft.AspNetCore.SignalR")
                                                                                 .GetType("Microsoft.Extensions.DependencyInjection.SignalRDependencyInjectionExtensions")
                                                                                 .GetMethod("AddHubOptions", BindingFlags.Static | BindingFlags.Public);
#pragma warning restore CS8601, CS8602

        private static readonly MethodInfo AddFilter_GenericMethod = typeof(HubOptionsExtensions).GetMethods(BindingFlags.Public | BindingFlags.Static)
                                                                                                 .First(m => m.Name == "AddFilter" &&
                                                                                                                        m.GetParameters().Length == 1 &&
                                                                                                                        m.GetParameters()[0].ParameterType == typeof(HubOptions) &&
                                                                                                                        m.IsGenericMethod);
    }
}

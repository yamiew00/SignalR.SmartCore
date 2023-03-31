using Microsoft.AspNetCore.SignalR;
using SignalR.SmartCore.Server;
using SignalR.SmartCore.Server.Attributes;
using SignalR.SmartCore.Server.DependencyInjections;
using SignalR.SmartCore.Server.DependencyInjections.Builders.Lifecycles;
using SignalR.SmartCore.Server.DependencyInjections.Builders.Options;
using SignalR.SmartCore.Server.Filters;
using SignalR.SmartCore.Server.Filters.Authenticators;
using SignalR.SmartCore.Server.Filters.SmartHubFilters;
using SignalR.SmartCore.Server.Managers;
using SignalR.SmartCore.Server.Providers;
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
                services.AddAsScoped(serviceType: typeof(ISmartHubAuthenticator),
                                     implementationType: authenticatorType);
            }

            /// For every Hubtype : SmartHub<IHubService>, it runs :
            /// signalRBuilder.AddHubOptions<HubType>(options =>
            ///               {
            ///                     //authenticator (if set)
            ///                     options.AddFilter<AuthenticationBaseFilter<HubType, IHubService>>();
            ///                     
            ///                     //custom Filters
            ///                     options.AddFilter<SmartHubFilterBase_IHubFilter_Adapter<CustomFilter, HubType>>
            ///               });
            foreach (Type hubType in hubTypes)
            {
                List<Action<HubOptions>> hubOptionList = new List<Action<HubOptions>>();

                //1. authenticator
                if (authenticatorType != null && hubType.GetCustomAttribute(typeof(AllowHubAnonymousAttribute)) == null)
                {
                    Type hubClientType = hubType.GetSmartHubGenericTypeParameter();
                    void configureAuthenticatorHubOptions(HubOptions options)
                    {
                        Type authenticationBaseFilterType = typeof(AuthenticationBaseFilter<,>).MakeGenericType(hubType, hubClientType);

                        MethodInfo addFilterMethod = AddFilter_GenericMethod.MakeGenericMethod(authenticationBaseFilterType);
                        addFilterMethod.Invoke(null, new object[] { options });
                    }

                    hubOptionList.Add(configureAuthenticatorHubOptions);
                }
                //2. custom filter
                if(smartCoreOptions.SmartHubOptionsDict.TryGetValue(hubType, out var smartHubOption))
                {
                    if (!smartHubOption.FilterTypes.Any()) continue;

                    
                    foreach (var smartHubFilterBaseType in smartHubOption.FilterTypes)
                    {
                        services.AddAsScoped(smartHubFilterBaseType);

                        // Construct a delegate for configuring HubOptions within AddHubOptions<THub>
                        void configureCustomHubOptions(HubOptions options)
                        {
                            Type adapterType = typeof(SmartHubFilterBase_IHubFilter_Adapter<,>).MakeGenericType(smartHubFilterBaseType, hubType);

                            MethodInfo addFilterMethod = AddFilter_GenericMethod.MakeGenericMethod(adapterType);
                            addFilterMethod.Invoke(null, new object[] { options });
                        }
                        hubOptionList.Add(configureCustomHubOptions);
                    }
                }

                //Last. execute 
                Action<HubOptions> combinedConfigureHubOptions = options =>
                {
                    foreach (var configureHubOptionsAction in hubOptionList)
                    {
                        configureHubOptionsAction(options);
                    }
                };
                MethodInfo addHubOptions_GenericMethod = AddHubOptions_GenericMethod.MakeGenericMethod(hubType);

                // .AddHubOption<TSmartHub>(opt => opt.AddFilter()... );
                addHubOptions_GenericMethod.Invoke(null, new object[] { signalRBuilder, combinedConfigureHubOptions });
            }

            //add manager
            services.AddAsSingleton(serviceType: typeof(ISmartHubManager),
                                  implementationInstance: new SmartHubManager(ServerGlobal.SmartHubConcreteTypes));

            //add non-generic hubProvider
            services.AddAsSingleton(serviceType: typeof(ISmartHubProvider),
                                    implementationType: typeof(SmartHubProvider));

            //add generic hubProvider
            foreach (var hubType in ServerGlobal.SmartHubConcreteTypes)
            {
                services.AddAsSingleton(serviceType: typeof(ISmartHubProvider<>).MakeGenericType(hubType),
                                        implementationType: typeof(SmartHubProvider<>).MakeGenericType(hubType));
            }

            return services;
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

        private static SmartHubLifecycleManager _LifeManager;
        private static bool UseLifeManager = false;


        public static IServiceCollection AddSmartSignalR(this IServiceCollection services,
                                                         out SmartHubLifecycleManager lifecycleManager,
                                                         Action<SmartCoreOptions> configure = default)
        {
            _LifeManager = new SmartHubLifecycleManager();
            UseLifeManager = true;

            var result = services.AddSmartSignalR(configure);
            lifecycleManager = _LifeManager;

            UseLifeManager = false;
            return result;  
        }

        private static IServiceCollection AddAsSingleton(this IServiceCollection services,
                                                         Type serviceType,
                                                         Type implementationType)
        {
            services.AddSingleton(serviceType, implementationType);
            if (UseLifeManager) _LifeManager.AddSingleton(serviceType, implementationType);
            return services;
        }

        private static IServiceCollection AddAsSingleton(this IServiceCollection services,
                                                         Type serviceType,
                                                         object implementationInstance)
        {
            services.AddSingleton(serviceType, implementationInstance);
            if (UseLifeManager) _LifeManager.AddSingleton(serviceType, implementationInstance);
            return services;
        }

        private static IServiceCollection AddAsScoped(this IServiceCollection services,
                                                      Type type)
        {
            services.AddScoped(type, type);
            if (UseLifeManager) _LifeManager.AddScoped(type, type);
            return services;
        }

        private static IServiceCollection AddAsScoped(this IServiceCollection services,
                                                      Type serviceType,
                                                      Type implementationType)
        {
            services.AddScoped(serviceType, implementationType);
            if (UseLifeManager) _LifeManager.AddScoped(serviceType, implementationType);
            return services;
        }
    }
}

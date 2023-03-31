using SignalR.SmartCore.Server.Filters.Authenticators;

namespace SignalR.SmartCore.Server.DependencyInjections.Builders.Options
{
    /// <summary>
    /// Options used to configure SmartHub instances.
    /// </summary>
    public class SmartCoreOptions
    {
        /// <summary>
        /// The type of the authentication layer. When the authentication layer is not added, this will be null.
        /// </summary>
        internal Type? AuthenticatorType { get; set; }

        //hubs
        internal Dictionary<Type, SmartHubOptions> SmartHubOptionsDict { get; } = new Dictionary<Type, SmartHubOptions>();

        public void AddAuthentication<T>() where T : ISmartHubAuthenticator
        {
            AuthenticatorType = typeof(T);
        }

        public void AddAuthentication(Type authenticatorType)
        {
            //check type: must be an implmentation of ISmartHubAuthenticator
            if (!typeof(ISmartHubAuthenticator).IsAssignableFrom(authenticatorType)) throw new Exception("authenticatorType must be an implementation of ISmartHubAuthenticator");

            AuthenticatorType = authenticatorType;
        }

        public SmartCoreOptions AddHubOptions<TSmartHub>(Action<SmartHubOptions> configure)
            where TSmartHub : ISmartHub
        {
            //the type TSmartHub must be a derived type of SmartHub<KClient>
            var SmartHubOptions = new SmartHubOptions();
            configure.Invoke(SmartHubOptions);

            SmartHubOptionsDict[typeof(TSmartHub)] = SmartHubOptions;
            return this;
        }
    }
}

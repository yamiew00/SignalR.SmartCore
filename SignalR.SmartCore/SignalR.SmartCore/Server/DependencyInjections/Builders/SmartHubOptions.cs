using SignalR.SmartCore.Server.Filters.Authenticators;

namespace SignalR.SmartCore.Server.DependencyInjections.Builders
{
    /// <summary>
    /// Options used to configure SmartHub instances.
    /// </summary>
    public class SmartHubOptions
    {
        private Type? _authenticatorType;

        internal Type AuthenticatorType => _authenticatorType ?? typeof(DefaultAuthenticator);

        public void AddAuthentication<T>() where T : ISmartHubAuthenticator
        {
            AddAuthentication(typeof(T));
        }

        public void AddAuthentication(Type authenticatorType)
        {
            //check type: must be an implmentation of ISmartHubAuthenticator
            if (!typeof(ISmartHubAuthenticator).IsAssignableFrom(authenticatorType)) throw new Exception("authenticatorType must be an implementation of ISmartHubAuthenticator");

            _authenticatorType = authenticatorType;
        }
    }
}

using Microsoft.AspNetCore.Http;

namespace SignalR.SmartCore.Server.Filters.Authenticators
{
    /// <summary>
    /// Default authenticator, it does not perform any checks during the authentication process.
    /// </summary>
    internal class DefaultAuthenticator : ISmartHubAuthenticator
    {
        public bool OnConnected(HttpContext context) => true;

        public bool OnMethodInvoked(HttpContext context) => true;        
    }
}

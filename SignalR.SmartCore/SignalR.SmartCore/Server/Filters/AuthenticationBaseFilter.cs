using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using SignalR.SmartCore.Server.Filters.Authenticators;
using SignalR.SmartCore.Server.Providers;

namespace SignalR.SmartCore.Server.Filters
{
    /// <summary>
    /// Basic authentication template filter for SmartHub
    /// </summary>
    public class AuthenticationBaseFilter<TSmartHub, KClient> : IHubFilter 
        where TSmartHub: SmartHub<KClient>
        where KClient : class
    {
        private readonly ISmartHubProvider<TSmartHub> SmartHubProvider;
        private readonly ISmartHubAuthenticator SmartHubAuthenticator;

        public AuthenticationBaseFilter(ISmartHubProvider<TSmartHub> smartHubProvider,
                                        ISmartHubAuthenticator smartHubAuthenticator)
        {
            this.SmartHubProvider = smartHubProvider;
            this.SmartHubAuthenticator = smartHubAuthenticator;
        }

        /// <summary>
        /// Authenticate when the connection is established.
        /// return brief error message when authenticate failed.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        /// <exception cref="HubException"></exception>
        public async Task OnConnectedAsync(HubLifetimeContext context,
                                           Func<HubLifetimeContext, Task> next)
        {
#pragma warning disable CS8600 
            HttpContext httpContext = context.Context.GetHttpContext();
#pragma warning restore CS8600 

            if (httpContext == null || !SmartHubAuthenticator.OnConnected(httpContext))
            {
                await context.Hub.Clients.Caller.SendError("Authentication failed");
                throw new HubException();
            }

            await next(context);
        }

#pragma warning disable CS8613 
        public async ValueTask<object> InvokeMethodAsync(HubInvocationContext invocationContext,
                                                         Func<HubInvocationContext, ValueTask<object>> next)
        {
#pragma warning disable CS8600 
            HttpContext httpContext = invocationContext.Context.GetHttpContext();
#pragma warning restore CS8600 
            if (httpContext == null || !SmartHubAuthenticator.OnMethodInvoked(httpContext))
            {
                DisconnectThisHub(invocationContext.Context.ConnectionId);
            }
            return await next(invocationContext);
        }
#pragma warning restore CS8613


        private void DisconnectThisHub(string connectionId)
        {
            var smartHub = SmartHubProvider.GetOnlineClients().FirstOrDefault(client => client.Context.ConnectionId == connectionId);
            smartHub?.ForceDisconnect();
        }
    }
}

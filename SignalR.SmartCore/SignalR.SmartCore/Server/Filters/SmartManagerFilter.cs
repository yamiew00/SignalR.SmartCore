using Microsoft.AspNetCore.SignalR;
using SignalR.SmartCore.Server.Managers;

namespace SignalR.SmartCore.Server.Filters
{
    internal class SmartManagerFilter : IHubFilter
    {
        private readonly ISmartHubManager SmartHubManager;

        public SmartManagerFilter(ISmartHubManager smartHubManager)
        {
            this.SmartHubManager = smartHubManager;
        }

        public async ValueTask<object> InvokeMethodAsync(HubInvocationContext invocationContext,
                                                         Func<HubInvocationContext, ValueTask<object>> next)
        {
            return await next(invocationContext);
        }

        public async Task OnConnectedAsync(HubLifetimeContext context,
                                           Func<HubLifetimeContext, Task> next)
        {
            await next(context);
            SmartHubManager.AddClient(context.Hub);
        }

        public async Task OnDisconnectedAsync(HubLifetimeContext context,
                                              Exception exception,
                                              Func<HubLifetimeContext, Exception, Task> next)
        {
            await next(context, exception);
            SmartHubManager.RemoveClient(context.Hub);
        }
    }
}

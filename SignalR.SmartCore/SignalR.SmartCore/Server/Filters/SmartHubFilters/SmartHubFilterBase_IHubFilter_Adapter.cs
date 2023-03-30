using Microsoft.AspNetCore.SignalR;

namespace SignalR.SmartCore.Server.Filters.SmartHubFilters
{
    /// <summary>
    /// The SmartHubFilterBase_IHubFilter_Adapter serves as an adapter that bridges the gap between SmartHubFilterBase and IHubFilter.
    /// It allows the derived classes of SmartHubFilterBase to be used in places where IHubFilter is expected, facilitating the integration with SignalR Hub pipelines.
    /// </summary>
    /// <typeparam name="TSmartHubFilterBase">the type SmartHubFilterBase must be a derived type of SmartHubFilterBase<TSmartHub>, which is TSmartHubFilterBase:  SmartHubFilterBase<TSmartHub> </typeparam>
    /// <typeparam name="TSmartHub">the type TSmartHub must be a derived type of SmartHub<KClient></typeparam>
    internal class SmartHubFilterBase_IHubFilter_Adapter<TSmartHubFilterBase, TSmartHub> : IHubFilter 
        where TSmartHubFilterBase : SmartHubFilterBase<TSmartHub>
        where TSmartHub : ISmartHub
    {
        private readonly TSmartHubFilterBase SmartHubFilterBase;

        public SmartHubFilterBase_IHubFilter_Adapter(TSmartHubFilterBase smartHubFilterBase)
        {
            this.SmartHubFilterBase = smartHubFilterBase;
        }

        public async Task OnConnectedAsync(HubLifetimeContext context, Func<HubLifetimeContext, Task> next)
        {
            await SmartHubFilterBase.OnConnectedAsync(Convert(context), () => next(context));   
        }

#pragma warning disable CS8613
        public ValueTask<object> InvokeMethodAsync(HubInvocationContext invocationContext, Func<HubInvocationContext, ValueTask<object>> next)
#pragma warning restore CS8613
        {
            return SmartHubFilterBase.InvokeMethodAsync(Convert(invocationContext), () => next(invocationContext));
        }

        public Task OnDisconnectedAsync(HubLifetimeContext context, Exception? exception, Func<HubLifetimeContext, Exception?, Task> next) 
        {
            return SmartHubFilterBase.OnDisconnectedAsync(Convert(context), () => next(context, exception));
        }

        private static SmartHubContext<TSmartHub> Convert(HubLifetimeContext hubLifetimeContext)
        {
            if (hubLifetimeContext.Hub is ISmartHub smartHub && smartHub is TSmartHub tSmartHub)
            {
                //hubLifetimeContext is guaranteed to receive a TSmartHub instance, as it's controlled by an external process.
#pragma warning disable CS8604
                return new SmartHubContext<TSmartHub>(connectionId: hubLifetimeContext.Context.ConnectionId,
                                                      httpContext: hubLifetimeContext.Context.GetHttpContext(),
                                                      smartHub: tSmartHub);
#pragma warning restore CS8604
            }

            throw new Exception($"Failed to cast {hubLifetimeContext.Hub.GetType()} to {typeof(TSmartHub)}.");
        }

        private static SmartHubContext<TSmartHub> Convert(HubInvocationContext hubInvocationContext)
        {
            if (hubInvocationContext.Hub is ISmartHub smartHub && smartHub is TSmartHub tSmartHub)
            {
                //hubLifetimeContext is guaranteed to receive a TSmartHub instance, as it's controlled by an external process.
#pragma warning disable CS8604
                return new SmartHubContext<TSmartHub>(connectionId: hubInvocationContext.Context.ConnectionId,
                                                      httpContext: hubInvocationContext.Context.GetHttpContext(),
                                                      smartHub: tSmartHub);
#pragma warning restore CS8604
            }

            throw new Exception($"Failed to cast {hubInvocationContext.Hub.GetType()} to {typeof(TSmartHub)}.");
        }
    }
}

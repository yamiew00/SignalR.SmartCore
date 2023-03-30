namespace SignalR.SmartCore.Server.Filters.SmartHubFilters
{
    /// <summary>
    /// SmartHubFilterBase is intended to be inherited and serves as the filter layer for SmartHub.
    /// </summary>
    /// <typeparam name="TSmartHub"></typeparam>
    public abstract class SmartHubFilterBase<TSmartHub>: ISmartHubFilterBase where TSmartHub: ISmartHub
    {
        public virtual Task OnConnectedAsync(SmartHubContext<TSmartHub> smartHubContext, Func<Task> _next) => _next();

        public virtual ValueTask<object> InvokeMethodAsync(SmartHubContext<TSmartHub> smartHubContext, Func<ValueTask<object>> _next) => _next();

        public virtual Task OnDisconnectedAsync(SmartHubContext<TSmartHub> smartHubContext, Func<Task> _next) => _next();
    }
}

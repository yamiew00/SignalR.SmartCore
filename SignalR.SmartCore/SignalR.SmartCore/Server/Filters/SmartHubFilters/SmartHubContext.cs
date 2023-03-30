using Microsoft.AspNetCore.Http;

namespace SignalR.SmartCore.Server.Filters.SmartHubFilters
{
    /// <summary>
    /// SmartHubContext carries the SmartHub, HttpContext, and its connectionId.
    /// </summary>
    /// <typeparam name="TSmartHub"> the type of SmartHub<TClient> </typeparam>
    public class SmartHubContext<TSmartHub> where TSmartHub: ISmartHub
    {
        public string ConnectionId { get;}

        public HttpContext HttpContext { get; }

        public TSmartHub SmartHub { get; }

        internal SmartHubContext(string connectionId,
                                 HttpContext httpContext,
                                 TSmartHub smartHub)
        {
            ConnectionId = connectionId;
            HttpContext = httpContext;
            SmartHub = smartHub;
        }
    }
}

using Microsoft.AspNetCore.SignalR;

namespace SignalR.SmartCore.Server.Providers
{
    /// <summary>
    /// The ISmartHubProvider interface provides online SmartHub instances.
    /// </summary>
    public interface ISmartHubProvider
    {
        public IEnumerable<Hub> GetOnlineClients { get; }
    }
}

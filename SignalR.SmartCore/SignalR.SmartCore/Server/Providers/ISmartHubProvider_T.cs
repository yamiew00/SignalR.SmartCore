using Microsoft.AspNetCore.SignalR;

namespace SignalR.SmartCore.Server.Providers
{
    /// <summary>
    /// The ISmartHubProvider interface provides a method to get the online SmartHub instances of a specific type.
    /// It ensures that the returned Hubs are of the given type and not all available Hubs. 
    /// </summary>
    /// <typeparam name="T"> The specific type of SmartHub to be returned.</typeparam>
    public interface ISmartHubProvider<T> where T : Hub
    {
        public IEnumerable<T> GetOnlineClients();
    }
}

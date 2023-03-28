using Microsoft.AspNetCore.SignalR;

namespace SignalR.SmartCore.Server.Managers
{
    internal interface ISmartHubManager
    {
        internal IEnumerable<Hub> AllClients { get; }

        internal IEnumerable<T> Clients<T>() where T : Hub;

        void AddClient<T>(T hub) where T : Hub;

        void RemoveClient<T>(T hub) where T : Hub;
    }
}

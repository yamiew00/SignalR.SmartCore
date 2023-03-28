using Microsoft.AspNetCore.SignalR;
using SignalR.SmartCore.Server.Managers;

namespace SignalR.SmartCore.Server.Providers
{
    internal class SmartHubProvider<T> : ISmartHubProvider<T> where T : Hub
    {
        private readonly ISmartHubManager SmartHubManager;

        public SmartHubProvider(ISmartHubManager smartHubManager)
        {
            this.SmartHubManager = smartHubManager;
        }

        public IEnumerable<T> GetOnlineClients() => SmartHubManager.Clients<T>();
    }
}

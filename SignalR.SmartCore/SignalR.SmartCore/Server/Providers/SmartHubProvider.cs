using Microsoft.AspNetCore.SignalR;
using SignalR.SmartCore.Server.Managers;

namespace SignalR.SmartCore.Server.Providers
{
    internal class SmartHubProvider : ISmartHubProvider
    {
        private readonly ISmartHubManager SmartHubManager;

        public SmartHubProvider(ISmartHubManager smartHubManager)
        {
            this.SmartHubManager = smartHubManager;
        }

        public IEnumerable<Hub> GetOnlineClients => SmartHubManager.AllClients;
    }
}

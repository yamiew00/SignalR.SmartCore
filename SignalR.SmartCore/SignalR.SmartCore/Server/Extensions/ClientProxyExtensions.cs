using Microsoft.AspNetCore.SignalR;

namespace SignalR.SmartCore.Server
{
    internal static class ClientProxyExtensions
    {
        internal static Task SendError(this IClientProxy clientProxy, string method, object? arg1, CancellationToken cancellationToken = default)
        {
            return clientProxy.SendCoreAsync(method, new[] { arg1 }, cancellationToken);
        }

        internal static Task SendError(this IClientProxy clientProxy, string errorMessage)
        {
            return clientProxy.SendCoreAsync(CommonConstants.ERROR_METHOD_NAME, new[] { errorMessage });
        }
    }
}

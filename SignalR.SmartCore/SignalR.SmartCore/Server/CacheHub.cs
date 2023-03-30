using Microsoft.AspNetCore.SignalR;
namespace SignalR.SmartCore.Server
{
    /// <summary>
    /// A Hub object implementing the Clone method.
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    public abstract class CacheHub<TClient> : Hub<TClient>, ICloneable where TClient : class
    {
        object ICloneable.Clone() => MemberwiseClone() as CacheHub<TClient>;

        /// <summary>
        /// force a client to be disconnected from server
        /// </summary>
        public void ForceDisconnect()
        {
            this.Context.Abort();
        }
    }
}

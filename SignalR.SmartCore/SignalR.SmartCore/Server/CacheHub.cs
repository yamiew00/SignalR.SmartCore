using Microsoft.AspNetCore.SignalR;
namespace SignalR.SmartCore.Server
{
    /// <summary>
    /// A Hub object implementing the Clone method.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class CacheHub<T> : Hub<T>, ICloneable where T : class
    {
        object ICloneable.Clone() => MemberwiseClone() as CacheHub<T>;

        /// <summary>
        /// force a client to be disconnected from server
        /// </summary>
        public void ForceDisconnect()
        {
            this.Context.Abort();
        }
    }
}

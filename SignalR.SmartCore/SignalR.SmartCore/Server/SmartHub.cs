using Microsoft.AspNetCore.SignalR;

namespace SignalR.SmartCore.Server
{
    /// <summary>
    /// A base class for a strongly typed SmartHub, which is a Hub.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SmartHub<T> : CacheHub<T> where T : class
    {
    }
}

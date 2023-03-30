namespace SignalR.SmartCore.Server
{
    /// <summary>
    /// A base class for a strongly typed SmartHub, which is a Hub.
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    public abstract class SmartHub<TClient> : CacheHub<TClient>, ISmartHub where TClient : class
    {
    }
}

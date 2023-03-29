namespace SignalR.SmartCore.Server.Attributes
{
    /// <summary>
    /// Specifies the class applied to does not require authorization.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class AllowHubAnonymousAttribute : Attribute
    {
    }
}

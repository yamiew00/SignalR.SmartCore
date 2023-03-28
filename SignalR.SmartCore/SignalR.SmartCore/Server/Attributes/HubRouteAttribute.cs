namespace SignalR.SmartCore.Server
{
    /// <summary>
    /// Specifies an attribute route on a SmartHub.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class HubRouteAttribute : Attribute
    {
        public string RouteName { get; }

        public HubRouteAttribute(string routeName)
        {
            RouteName = routeName;
        }
    }
}

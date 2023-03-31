namespace SignalR.SmartCore.Server.DependencyInjections.Builders.Lifecycles
{
    public class Registration
    {
        public object? Instance { get; set; }

        public Type ServiceType { get; set; }

        public Type? ImplmententType { get; set; }

        public SmartHubLifecycle Lifecycle { get; set; }
    }
}
